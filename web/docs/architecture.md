# OfflinePay Architecture

## 1. Architecture Overview

OfflinePay follows a client-server architecture with an offline-first transaction model.

The system consists of:

- React web application
- React Native mobile application
- ASP.NET Core Web API
- PostgreSQL database
- Redis cache
- Azure Service Bus
- Azure Functions
- Azure API Management
- Azure Key Vault
- Application Insights
- GitHub Actions for CI/CD

### High-Level Architecture

```text
                    ┌─────────────────────────┐
                    │      React Web App      │
                    │  Customer / Merchant /  │
                    │          Admin          │
                    └────────────┬────────────┘
                                 │
                                 │ HTTPS
                                 ▼
                    ┌─────────────────────────┐
                    │ Azure API Management    │
                    │                         │
                    │ Routing / Policies /    │
                    │ Rate Limiting / Auth    │
                    └────────────┬────────────┘
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │   ASP.NET Core Web API  │
                    │                         │
                    │ Authentication          │
                    │ Wallet                   │
                    │ Payments                 │
                    │ Transactions             │
                    │ Offline Payments         │
                    │ Synchronization          │
                    └──────┬──────────┬───────┘
                           │          │
              ┌────────────┘          └─────────────┐
              ▼                                     ▼
    ┌──────────────────┐                   ┌──────────────────┐
    │   PostgreSQL     │                   │      Redis       │
    │                  │                   │                  │
    │ Users            │                   │ Cache            │
    │ Wallets          │                   │ Rate Limiting    │
    │ Transactions     │                   │ Short-lived Data │
    │ Offline Allowance│                   │                  │
    └──────────────────┘                   └──────────────────┘
                           │
                           │ Async Processing
                           ▼
                  ┌──────────────────────┐
                  │   Azure Service Bus  │
                  │                      │
                  │ Sync Events          │
                  │ Settlement           │
                  │ Notifications        │
                  └──────────┬───────────┘
                             │
                             ▼
                  ┌──────────────────────┐
                  │   Azure Functions    │
                  │                      │
                  │ Transaction Worker   │
                  │ Sync Worker          │
                  │ Notification Worker  │
                  └──────────┬───────────┘
                             │
                             ▼
                    ┌──────────────────┐
                    │   Application    │
                    │    Insights      │
                    │                  │
                    │ Logs / Metrics /  │
                    │ Exceptions /     │
                    │ Performance      │
                    └──────────────────┘


             ┌──────────────────────────┐
             │   React Native Mobile   │
             │                          │
             │ Wallet                   │
             │ QR Payments              │
             │ Offline Payment Engine   │
             │ Local Transaction Store  │
             │ Secure Device Keys      │
             │ Sync Engine             │
             └──────────────────────────┘
```

## 2. Offline Payment Architecture

The offline payment capability is the core feature of OfflinePay.

The objective is to allow an eligible customer to make a limited-value payment to a merchant even when the customer's device does not have internet connectivity.

The system does not treat an offline transaction as immediately settled.

Instead, the transaction follows two stages:

1. Local acceptance
2. Server validation and settlement after connectivity is restored

### 2.1 Offline Payment Concept

Before going offline, the server provides the customer's device with a limited offline spending allowance.

Example:

```text
Online

Customer Wallet Balance
        ₹10,000
             │
             ▼
Server authorizes
Offline Allowance
             │
             ▼
        ₹1,000


```

## 3. Transaction Integrity and Double-Spending Mitigation

### 3.1 The Double-Spending Problem

Offline payments introduce a fundamental distributed-systems problem.

When a device is connected to the server, the server can immediately verify:

- Current wallet balance
- Previous transactions
- Available funds
- Transaction status
- Duplicate requests

When the device is offline, the server cannot immediately observe the transaction.

For example:

```text
Customer Wallet = ₹1,000

              No Internet
                  │
          ┌───────┴────────┐
          │                │
          ▼                ▼
      Merchant A        Merchant B
        ₹800              ₹800
```

## 4. Online Payment Architecture

Online payments are the standard payment path when the customer has an active internet connection.

Unlike an offline payment, the server can immediately validate the transaction against the authoritative wallet and transaction state.

### 4.1 Online Payment Flow

The basic flow is:

```text
Customer
   │
   │ 1. Scan Merchant QR
   ▼
React Native App
   │
   │ 2. Payment Request
   ▼
ASP.NET Core API
   │
   ├── Authenticate User
   │
   ├── Validate Merchant
   │
   ├── Validate Amount
   │
   ├── Check Wallet Balance
   │
   ├── Check Idempotency
   │
   └── Create Transaction
            │
            ▼
       Database Transaction
            │
            ├── Debit Customer
            │
            ├── Credit Merchant
            │
            └── Store Transaction
            │
            ▼
       Transaction Settled
            │
            ├───────────────┐
            ▼               ▼
      Customer App      Merchant App
      Notification      Notification
```

## 5. Synchronization Architecture

Synchronization is responsible for transferring locally stored offline transactions to the backend when network connectivity becomes available.

The synchronization process must be reliable, retryable, idempotent, and recoverable.

### 5.1 Synchronization Flow

The overall flow is:

```text
React Native App
      │
      │ Offline
      ▼
Local Transaction Store
      │
      │ Internet Restored
      ▼
Sync Engine
      │
      ▼
ASP.NET Core API
      │
      │ Validate Request
      ▼
Azure Service Bus
      │
      ▼
Transaction Processing Function
      │
      ▼
Server Validation
      │
      ├───────────────┐
      ▼               ▼
   Valid            Invalid
      │               │
      ▼               ▼
  Settlement       Rejection
      │
      ▼
Database
      │
      ▼
Sync Result
      │
      ▼
React Native App
```

## 6. Data Architecture

OfflinePay uses different storage mechanisms for different responsibilities.

The primary source of truth for financial and transaction data is the backend database.

The mobile application maintains a local persistent store for offline operation.

Redis is used only for temporary or performance-related data and is never treated as the authoritative source for financial transactions.

### 6.1 Storage Responsibilities

```text
┌─────────────────────────────────────────────────────────────┐
│                     OfflinePay Storage                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ PostgreSQL                                                  │
│ ├── Users                                                   │
│ ├── Devices                                                 │
│ ├── Wallets                                                 │
│ ├── Offline Allowances                                      │
│ ├── Payment Requests                                        │
│ ├── Transactions                                             │
│ ├── Transaction Attempts                                    │
│ └── Audit Logs                                              │
│                                                             │
│ Redis                                                       │
│ ├── Short-lived cache                                       │
│ ├── Rate limiting data                                      │
│ ├── Temporary payment request data                          │
│ └── Dashboard/cache data                                    │
│                                                             │
│ Mobile Local Storage                                       │
│ ├── Offline Allowance                                       │
│ ├── Pending Transactions                                    │
│ ├── Transaction Queue                                       │
│ ├── Sequence Number                                         │
│ └── Synchronization State                                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## 7. API Architecture

The ASP.NET Core Web API is the primary communication layer between the client applications and the backend services.

The API is responsible for authentication, authorization, request validation, business operations, transaction creation, synchronization, and communication with asynchronous processing components.

### 7.1 API Design Principles

The API follows these principles:

1. RESTful resource-oriented endpoints
2. HTTPS for all communication
3. Authentication using access tokens
4. Role-based authorization
5. Input validation at the API boundary
6. Consistent request and response contracts
7. Idempotency for payment and synchronization operations
8. Correlation IDs for request tracing
9. Appropriate HTTP status codes
10. API versioning
11. No sensitive information in responses or logs
12. Business rules enforced on the server

The client must never be trusted to enforce security-sensitive business rules.

---

### 7.2 API Base Structure

The initial API version will use:

```text
/api/v1
```

## 8. Deployment and Azure Architecture

OfflinePay is designed to run locally during development and can be deployed to Azure using managed cloud services.

The architecture separates application hosting, data storage, asynchronous processing, secrets, monitoring, and API management.

### 8.1 Azure High-Level Architecture

```text
                              Internet
                                 │
                                 ▼
                     ┌──────────────────────┐
                     │ Azure API Management │
                     │                      │
                     │ API Gateway          │
                     │ Rate Limiting        │
                     │ Policies             │
                     │ API Versioning       │
                     └──────────┬───────────┘
                                │
                                ▼
                     ┌──────────────────────┐
                     │ ASP.NET Core API     │
                     │                      │
                     │ Authentication       │
                     │ Payments             │
                     │ Wallets              │
                     │ Transactions         │
                     │ Synchronization      │
                     └───────┬───────┬──────┘
                             │       │
                 ┌───────────┘       └────────────┐
                 ▼                                ▼
        ┌─────────────────┐              ┌─────────────────┐
        │ Azure Database  │              │ Azure Cache for │
        │ for PostgreSQL  │              │ Redis           │
        │                 │              │                 │
        │ Source of Truth │              │ Cache / Temp    │
        └────────┬────────┘              └─────────────────┘
                 │
                 │
                 ▼
        ┌─────────────────────┐
        │ Azure Service Bus   │
        │                     │
        │ Transaction Events  │
        │ Sync Messages       │
        │ Notifications       │
        └──────────┬──────────┘
                   │
                   ▼
        ┌─────────────────────┐
        │ Azure Functions     │
        │                     │
        │ Transaction Worker  │
        │ Sync Worker         │
        │ Notification Worker │
        └──────────┬──────────┘
                   │
                   ▼
        ┌─────────────────────┐
        │ Application         │
        │ Insights            │
        │                     │
        │ Logs / Metrics      │
        │ Traces / Errors     │
        └─────────────────────┘


        ┌─────────────────────┐
        │ Azure Key Vault     │
        │                     │
        │ Secrets             │
        │ Connection Strings  │
        │ Certificates        │
        └─────────────────────┘
```
