# OfflinePay — Requirements

## 1. Project Overview

### 1.1 Purpose

OfflinePay is a digital payment platform designed to support limited-value transactions in environments where internet connectivity is unavailable or unreliable.

The primary goal is to allow a customer and merchant to exchange a payment transaction locally without requiring an active internet connection at the moment of payment. Once connectivity is restored, the transaction is synchronized with the backend, validated, and processed for settlement.

> **Important:** OfflinePay is a technical prototype for learning and demonstrating offline-first payment architecture. It is not intended to replace UPI, banking infrastructure, or regulated payment systems.

### 1.2 Problem Statement

Traditional digital payment applications generally depend on network connectivity to communicate with payment servers and financial institutions.

In remote locations such as:

- Mountains
- Rural areas
- Highways
- Forest regions
- Areas affected by network outages
- Locations with unstable mobile connectivity

users may be unable to complete digital payments even when they have sufficient funds.

OfflinePay aims to explore a solution for this problem through:

- Offline transaction processing
- Secure local transaction storage
- Limited offline spending allowance
- Device-to-device transaction exchange
- Transaction synchronization
- Server-side validation
- Duplicate transaction detection
- Secure transaction signing

### 1.3 Project Goals

The primary goals of OfflinePay are:

1. Build a modern digital payment application using React, React Native, ASP.NET Core and Azure.
2. Learn React and TypeScript through a real-world project.
3. Learn React Native and mobile offline-first development.
4. Design an offline-capable transaction system.
5. Implement secure transaction synchronization.
6. Explore distributed-system concepts such as eventual consistency and idempotency.
7. Implement asynchronous processing using Azure Service Bus and Azure Functions.
8. Build a production-style cloud architecture using Microsoft Azure.
9. Implement automated testing and CI/CD.
10. Create a portfolio project that demonstrates practical software engineering and system-design skills.

## 2. Users and Roles

OfflinePay will initially support three types of users:

### 2.1 Customer

A customer is a user who can hold funds in their OfflinePay wallet and make payments to merchants.

#### Customer capabilities

- Register an account
- Log in securely
- View wallet balance
- View available offline spending allowance
- Add simulated funds to the wallet
- Send money to another user
- Scan a merchant QR code
- Make online payments
- Make eligible offline payments
- View transaction history
- View pending offline transactions
- Synchronize pending transactions when connectivity is restored
- View payment status
- Manage personal profile
- Manage registered devices
- View security information

### 2.2 Merchant

A merchant is a user who accepts payments from customers.

#### Merchant capabilities

- Register a merchant account
- Log in securely
- View wallet balance
- Generate a payment QR code
- Request a specific payment amount
- Receive online payments
- Receive eligible offline payments
- View received transactions
- View pending synchronization transactions
- Synchronize offline transactions
- View transaction status
- View basic transaction reports
- Manage merchant profile
- Manage registered devices

### 2.3 Administrator

An administrator manages and monitors the OfflinePay platform.

#### Administrator capabilities

- Manage users
- Manage merchant accounts
- View system transactions
- Search and filter transactions
- View transaction details
- Monitor failed and rejected transactions
- Monitor synchronization failures
- Review suspicious transactions
- View audit logs
- Manage system configuration
- Monitor application health
- View system metrics

### 2.4 Role-Based Access Control

OfflinePay will use role-based authorization.

The initial roles are:

| Role     | Description                       |
| -------- | --------------------------------- |
| Customer | Makes and receives payments       |
| Merchant | Accepts customer payments         |
| Admin    | Manages and monitors the platform |

Users must only be able to access functionality permitted for their assigned role.

## 3. Functional Requirements

### 3.1 Authentication and User Management

The system shall provide secure user authentication and account management.

#### Requirements

- Users shall be able to register an account.
- Users shall be able to log in using their registered credentials.
- The system shall authenticate users using secure authentication mechanisms.
- The system shall issue an access token after successful authentication.
- The system shall support role-based authorization.
- Users shall only be able to access functionality permitted for their role.
- Users shall be able to view and update their profile information.
- Users shall be able to view their registered devices.
- The system shall maintain an audit trail for security-sensitive operations.

---

### 3.2 Wallet Management

The system shall provide a digital wallet for customers and merchants.

#### Requirements

- Users shall be able to view their wallet balance.
- Users shall be able to view their available balance.
- The prototype shall support simulated wallet top-ups.
- The system shall maintain a transaction history for wallet operations.
- Wallet balance changes shall be recorded as transactions.
- The system shall prevent transactions when sufficient available funds are not present.
- Wallet operations shall be processed atomically.

> **Prototype limitation:** Wallet funding and settlement will be simulated. The application will not connect to real bank accounts or process real money.

---

### 3.3 Online Payments

When internet connectivity is available, users shall be able to perform online payments.

#### Payment Flow

```text
Customer
    |
    v
React / React Native
    |
    v
ASP.NET Core API
    |
    v
Transaction Service
    |
    +--> Validate Customer
    |
    +--> Validate Balance
    |
    +--> Validate Merchant
    |
    +--> Create Transaction
    |
    +--> Update Wallets
    |
    v
Database
    |
    v
Payment Result
```

## 4. Non-Functional Requirements

### 4.1 Security

Security is a critical requirement because OfflinePay handles financial transaction data.

The system shall:

- Use HTTPS for all network communication.
- Never store passwords in plain text.
- Use secure password hashing.
- Use JWT-based authentication for API access.
- Implement role-based authorization.
- Validate and sanitize all API inputs.
- Protect sensitive configuration using secure secret storage.
- Use device-bound cryptographic keys for offline transactions.
- Digitally sign eligible offline transactions.
- Verify transaction signatures on the server.
- Protect against transaction replay attacks.
- Protect against duplicate transaction submission.
- Protect against unauthorized device access.
- Maintain security-related audit logs.
- Never expose private cryptographic keys to the backend or other users.

> **Prototype security note:** The project will demonstrate secure transaction architecture, but it will not be used to process real financial transactions.

---

### 4.2 Offline Reliability

Offline functionality is the primary differentiating feature of OfflinePay.

The application shall:

- Continue to operate when internet connectivity is unavailable.
- Store eligible offline transactions locally.
- Maintain a reliable local transaction queue.
- Prevent loss of locally stored transactions.
- Retry failed synchronization attempts.
- Resume interrupted synchronization.
- Avoid creating duplicate transactions during retries.
- Clearly display whether a transaction is offline-pending or server-settled.
- Preserve transaction ordering where required.
- Detect and report synchronization conflicts.

---

### 4.3 Data Integrity

The system shall maintain transaction integrity across online and offline operations.

The system shall:

- Generate unique transaction identifiers.
- Maintain transaction sequence numbers where required.
- Use server-side validation before settlement.
- Use database transactions for atomic wallet operations.
- Prevent partial wallet updates.
- Implement idempotency for payment and synchronization APIs.
- Detect duplicate transactions.
- Detect tampered transaction data.
- Maintain an immutable audit history for completed transactions.

---

### 4.4 Performance

The application should provide responsive user interactions.

Target guidelines for the prototype:

| Operation                           | Target                              |
| ----------------------------------- | ----------------------------------- |
| Local UI interaction                | < 100 ms                            |
| Local offline transaction creation  | < 500 ms                            |
| Normal API response                 | < 1 second                          |
| Transaction synchronization request | < 2 seconds under normal conditions |
| Dashboard initial load              | < 2 seconds under normal conditions |

These are development targets rather than production SLAs.

---

### 4.5 Scalability

The backend shall be designed so that individual components can scale independently.

The architecture should support:

- Horizontal API scaling.
- Asynchronous transaction processing.
- Queue-based workloads.
- Distributed caching.
- Database optimization.
- Independent scaling of background processors.
- Stateless API instances where possible.

Azure Service Bus shall be used to decouple transaction submission from asynchronous processing.

---

### 4.6 Availability

The online system should remain available even if individual application components experience temporary failures.

The system shall support:

- Retry mechanisms.
- Health checks.
- Graceful error handling.
- Queue-based asynchronous processing.
- Database transaction consistency.
- Service monitoring.
- Application health monitoring.

The offline client shall continue supporting eligible local operations even when the backend is temporarily unavailable.

---

### 4.7 Reliability

The system shall be designed to avoid transaction loss.

The system shall:

- Persist transactions before acknowledging them as stored locally.
- Retry failed network requests.
- Use idempotency keys for retryable operations.
- Avoid processing the same transaction more than once.
- Maintain transaction states throughout processing.
- Record failed processing attempts.
- Support recovery after application restart.
- Support recovery after temporary network loss.

---

### 4.8 Observability

The system shall provide sufficient logging and monitoring to understand application behavior.

The system shall capture:

- API requests and responses where appropriate.
- Authentication events.
- Transaction processing events.
- Offline synchronization events.
- Failed transactions.
- Rejected transactions.
- Retry attempts.
- Service Bus processing failures.
- Application exceptions.
- Performance metrics.

Azure Application Insights will be used for cloud application monitoring.

Sensitive information such as passwords, private keys, authentication tokens, and other secrets must never be written to application logs.

---

### 4.9 Maintainability

The application shall follow clean and maintainable coding practices.

The project shall:

- Follow established .NET and TypeScript coding conventions.
- Use meaningful names for classes, methods, variables, and components.
- Separate business logic from presentation logic.
- Use dependency injection.
- Keep API contracts clearly defined.
- Avoid unnecessary duplication.
- Use automated tests for important business logic.
- Maintain technical documentation.
- Use Git for source control.
- Use meaningful commit messages.
- Use pull requests for significant feature changes where appropriate.

---

### 4.10 Testability

The application shall be designed to support automated testing.

Testing shall include:

- Unit tests.
- API/integration tests.
- React component tests.
- React Native tests.
- Transaction business-rule tests.
- Offline transaction tests.
- Synchronization tests.
- Duplicate transaction tests.
- Failure and retry tests.

Critical payment logic should have automated test coverage before being considered complete.

---

### 4.11 Privacy

The system shall minimize the storage of personal and sensitive information.

The application shall:

- Store only required user information.
- Protect sensitive information.
- Avoid logging personal or financial information unnecessarily.
- Provide appropriate access controls.
- Avoid exposing another user's transaction details.
- Separate authentication data from application data where appropriate.

---

### 4.12 Disaster Recovery and Data Recovery

The online backend shall be designed with recovery in mind.

The system should support:

- Database backups.
- Transaction recovery.
- Retry of failed asynchronous messages.
- Dead-letter handling for failed Service Bus messages.
- Recovery of pending transactions.
- Application restart without losing committed transaction state.

Offline transactions must remain locally recoverable until they are successfully synchronized or explicitly rejected.

## 5. Technology Architecture

### 5.1 Frontend Applications

OfflinePay will have two client applications.

#### Web Application

The web application will be built using:

- React
- TypeScript
- Vite
- React Router
- ESLint

The web application will primarily be used for:

- Customer web access
- Merchant management
- Administrative functionality
- Transaction history
- Dashboard and reporting
- System monitoring

#### Mobile Application

The mobile application will be built using:

- React Native
- TypeScript

The mobile application will be the primary client for offline payment functionality.

It will provide access to:

- Wallet
- Online payments
- Offline payments
- QR scanning
- Transaction history
- Offline transaction queue
- Synchronization
- Device security features

---

### 5.2 Backend

The backend will be developed using:

- ASP.NET Core Web API
- C#
- Entity Framework Core
- REST APIs
- Dependency Injection
- JWT authentication
- Role-based authorization

The backend will contain the core business logic for:

- User management
- Authentication
- Wallet management
- Payment processing
- Transaction management
- Offline transaction validation
- Synchronization
- Settlement processing
- Audit logging

The API should remain stateless wherever possible.

---

### 5.3 Database

The application will initially use PostgreSQL during local development.

The database will store:

- Users
- Roles
- Devices
- Wallets
- Transactions
- Offline transactions
- Payment requests
- Audit records
- Synchronization records

The production Azure environment will use Azure-managed database infrastructure.

The final database choice will be evaluated based on application requirements, Azure integration, cost, scalability, and operational simplicity.

---

### 5.4 Caching

Redis will be used for data that benefits from fast temporary access.

Potential use cases include:

- Frequently accessed reference data
- Session-related information where appropriate
- Rate limiting
- Distributed locks where required
- Short-lived payment request data
- Frequently accessed dashboard information

Redis shall not be treated as the primary source of truth for financial transaction data.

---

### 5.5 Messaging

Azure Service Bus will be used for asynchronous communication between application components.

Potential use cases include:

- Offline transaction synchronization
- Settlement processing
- Notification processing
- Background transaction validation
- Retryable operations
- Integration between backend services

Example:

```text
ASP.NET Core API
       |
       v
Azure Service Bus
       |
       v
Azure Function
       |
       v
Transaction Processor
       |
       v
Database
```
