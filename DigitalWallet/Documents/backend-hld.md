# Backend High-Level Design (HLD) — Digital Wallet

## Table of Contents
1. [Business Context](#1-business-context)
2. [System Goals & Constraints](#2-system-goals--constraints)
3. [Service Responsibilities](#3-service-responsibilities)
4. [Service Dependency Graph](#4-service-dependency-graph)
5. [API Gateway Routing](#5-api-gateway-routing)
6. [Key User Flows](#6-key-user-flows)
7. [Event Bus Topology](#7-event-bus-topology)
8. [Database-Per-Service Strategy](#8-database-per-service-strategy)
9. [Cross-Cutting Concerns](#9-cross-cutting-concerns)
10. [Non-Functional Requirements](#10-non-functional-requirements)

---

## 1. Business Context

The Digital Wallet is a multi-service fintech backend that enables users to:
- Register and verify their identity via KYC (Know Your Customer)
- Maintain a digital wallet with a real-time balance
- Transfer money to other users
- Top-up their wallet via a payment gateway
- Earn loyalty reward points on transactions
- Receive real-time in-app notifications
- Raise and track support tickets
- Administrators can manage KYC approvals, view dashboards, and manage reward catalogs

### Actors

| Actor | Role |
|-------|------|
| **End User** | Registers, submits KYC, transfers money, earns rewards, raises tickets |
| **Admin** | Reviews KYC submissions, manages reward catalog, handles support tickets, views dashboards |
| **System** | Internal services communicating via HTTP and RabbitMQ events |

---

## 2. System Goals & Constraints

### Goals
- Provide a secure, scalable microservices backend for digital wallet operations
- Enforce KYC before wallet activation to comply with financial regulations
- Decouple services using an event-driven architecture (RabbitMQ)
- Enable independent deployment and scaling of each service
- Provide a unified API entry point via API Gateway

### Constraints
- Each service owns its own database (no shared DB)
- Cross-service data access only via API calls or events
- All external-facing APIs require JWT authentication (except register/login/OTP)
- Wallet transfers are synchronous; reward/notification side-effects are async
- Admin operations require `Admin` role in the JWT claims

---

## 3. Service Responsibilities

```mermaid
graph TD
    subgraph "Auth Service"
        AU1["User Registration & Login"]
        AU2["OTP Generation & Verification"]
        AU3["KYC Document Submission"]
        AU4["JWT + Refresh Token Management"]
        AU5["User Status Updates (via Admin)"]
    end

    subgraph "Wallet Service"
        WA1["Wallet Account Management"]
        WA2["Balance Queries"]
        WA3["Money Transfers (P2P)"]
        WA4["Wallet Top-Up (from PaymentGateway)"]
        WA5["Ledger / Transaction History"]
        WA6["Daily Transfer Limit Enforcement"]
    end

    subgraph "Rewards Service"
        RE1["Rewards Account per User"]
        RE2["Points Earning (on TopUp/Transfer)"]
        RE3["Tier Progression (Bronze→Platinum)"]
        RE4["Rewards Catalog Management"]
        RE5["Redemption Processing"]
    end

    subgraph "Notification Service"
        NO1["In-App Notification Delivery"]
        NO2["Notification Template Management"]
        NO3["Notification Log Storage"]
        NO4["Mark Read / Unread"]
    end

    subgraph "Admin Service"
        AD1["KYC Review Queue"]
        AD2["KYC Approve / Reject"]
        AD3["Dashboard Metrics"]
        AD4["Rewards Catalog CRUD"]
        AD5["Admin Activity Logging"]
    end

    subgraph "Support Ticket Service"
        ST1["User Ticket Submission"]
        ST2["Threaded Replies"]
        ST3["Admin Resolution"]
        ST4["Ticket Status Tracking"]
    end
```

---

## 4. Service Dependency Graph

```mermaid
graph TD
    GW["API Gateway"]

    GW --> AUTH
    GW --> WALLET
    GW --> REWARDS
    GW --> NOTIFY
    GW --> ADMIN
    GW --> SUPPORT

    WALLET -->|"HTTP: resolve user by email"| AUTH
    WALLET -->|"HTTP: award points"| REWARDS
    ADMIN -->|"HTTP: update user status"| AUTH
    ADMIN -->|"HTTP: create wallet"| WALLET

    AUTH -->|"Event: UserRegistered\nKYCSubmitted\nOtpGenerated"| RMQ[("RabbitMQ")]
    WALLET -->|"Event: TopUpCompleted\nTransferCompleted\nPaymentFailed"| RMQ
    ADMIN -->|"Event: KYCApproved\nKYCRejected"| RMQ
    SUPPORT -->|"Event: TicketCreated"| RMQ
    REWARDS -->|"Event: PointsEarned\nRedemptionCompleted"| RMQ

    RMQ -->|"Consume"| REWARDS
    RMQ -->|"Consume"| NOTIFY
    RMQ -->|"Consume"| AUTH

    style GW fill:#f0a500,color:#000
    style RMQ fill:#ff6b35,color:#fff
```

---

## 5. API Gateway Routing

The Ocelot gateway is the single entry point on port **5000**. All downstream services are only reachable internally.

| Gateway Route Pattern | Downstream Service | Notes |
|----------------------|--------------------|-------|
| `/api/auth/**` | Auth Service | Register, Login, OTP, KYC |
| `/api/otp/**` | Auth Service | OTP endpoints |
| `/api/kyc/**` | Auth Service | KYC submission |
| `/api/wallet/**` | Wallet Service | Balance, Transfer, Top-Up |
| `/api/reward/**` | Rewards Service | Points, Tiers, Redemptions |
| `/api/notifications/**` | Notification Service | Notifications |
| `/api/admin/**` | Admin Service | KYC Review, Dashboard |
| `/api/support/**` | Support Ticket Service | User & Admin tickets |

**Gateway responsibilities:**
- JWT token validation before forwarding
- HTTPS termination
- CORS policy enforcement
- Request/response logging (Serilog)

---

## 6. Key User Flows

### 6.1 User Registration & KYC Approval

```mermaid
sequenceDiagram
    actor User
    participant GW as API Gateway
    participant AUTH as Auth Service
    participant MQ as RabbitMQ
    participant ADMIN as Admin Service
    participant WALLET as Wallet Service
    participant NOTIFY as Notification Service

    User->>GW: POST /api/auth/register {name, email, password, phone}
    GW->>AUTH: Forward request
    AUTH->>AUTH: Hash password, create User (status=Pending)
    AUTH-->>User: JWT token + user details

    User->>GW: POST /api/kyc/submit {docType, docNumber, docImage}
    GW->>AUTH: Forward (JWT validated)
    AUTH->>AUTH: Save KYCDocument (status=Pending)
    AUTH->>MQ: Publish KYCSubmitted event
    MQ->>ADMIN: KYCSubmittedConsumer: sync KYC record

    Note over ADMIN: Admin reviews in dashboard

    ADMIN->>AUTH: PUT /api/auth/update-status (userId, Active)
    ADMIN->>WALLET: POST /api/wallet/create-internal (userId)
    WALLET-->>ADMIN: WalletAccount created
    ADMIN->>MQ: Publish KYCApproved event
    MQ->>NOTIFY: Save "KYC Approved" notification
    MQ->>AUTH: Update KYC status in Auth DB

    User->>GW: GET /api/notifications
    GW->>NOTIFY: Return notifications
```

---

### 6.2 Wallet Top-Up Flow

```mermaid
sequenceDiagram
    actor User
    participant GW as API Gateway
    participant WALLET as Wallet Service
    participant MQ as RabbitMQ
    participant REWARDS as Rewards Service
    participant NOTIFY as Notification Service

    User->>GW: POST /api/wallet/topup {amount, paymentMethod}
    GW->>WALLET: Forward (JWT validated)
    WALLET->>WALLET: Validate wallet is Active
    WALLET->>WALLET: Process payment (Mock Gateway)
    WALLET->>WALLET: Credit wallet balance
    WALLET->>WALLET: Create LedgerEntry (TopUp)
    WALLET->>MQ: Publish TopUpCompleted {userId, amount, newBalance}
    WALLET-->>User: TopUp success + new balance

    par Async side-effects
        MQ->>REWARDS: TopUpCompletedConsumer: award points
        REWARDS->>REWARDS: Calculate points (based on amount)
        REWARDS->>REWARDS: Update RewardsAccount + tier
        REWARDS->>MQ: Publish PointsEarned event
    and
        MQ->>NOTIFY: Save "Top-Up Successful" notification
    and
        MQ->>NOTIFY: Save "Points Earned" notification (from PointsEarned event)
    end
```

---

### 6.3 Peer-to-Peer Transfer

```mermaid
sequenceDiagram
    actor Sender
    participant GW as API Gateway
    participant WALLET as Wallet Service
    participant AUTH as Auth Service
    participant MQ as RabbitMQ
    participant REWARDS as Rewards Service
    participant NOTIFY as Notification Service

    Sender->>GW: POST /api/wallet/transfer {recipientEmail, amount, note}
    GW->>WALLET: Forward (JWT validated)
    WALLET->>AUTH: GET /api/auth/user-by-email?email=...
    AUTH-->>WALLET: recipientUserId
    WALLET->>WALLET: Check sender balance >= amount
    WALLET->>WALLET: Check daily/monthly limits
    WALLET->>WALLET: Debit sender wallet
    WALLET->>WALLET: Credit recipient wallet
    WALLET->>WALLET: Create LedgerEntry for both wallets
    WALLET->>MQ: Publish TransferCompleted {senderId, recipientId, amount}
    WALLET-->>Sender: Transfer success + new balance

    par Async side-effects
        MQ->>REWARDS: TransferCompletedConsumer: award sender points
        REWARDS->>REWARDS: Update tier if threshold crossed
        REWARDS->>MQ: Publish PointsEarned
    and
        MQ->>NOTIFY: "Transfer Sent" for sender
    and
        MQ->>NOTIFY: "Transfer Received" for recipient
    end
```

---

### 6.4 OTP Verification

```mermaid
sequenceDiagram
    actor User
    participant GW as API Gateway
    participant AUTH as Auth Service
    participant MQ as RabbitMQ
    participant NOTIFY as Notification Service

    User->>GW: POST /api/otp/send {email}
    GW->>AUTH: Forward request
    AUTH->>AUTH: Generate numeric OTP
    AUTH->>AUTH: Save OTPLog (expires in 5 min)
    AUTH->>MQ: Publish OtpGenerated {userId, otp, email}
    MQ->>NOTIFY: Deliver OTP notification
    AUTH-->>User: OTP sent

    User->>GW: POST /api/otp/verify {email, otp}
    GW->>AUTH: Forward request
    AUTH->>AUTH: Validate OTP (not expired, matches)
    AUTH->>AUTH: Mark OTPLog as used
    AUTH-->>User: Verification success
```

---

### 6.5 Support Ticket Flow

```mermaid
sequenceDiagram
    actor User
    actor Admin
    participant GW as API Gateway
    participant SUPPORT as Support Ticket Service
    participant MQ as RabbitMQ
    participant NOTIFY as Notification Service

    User->>GW: POST /api/support/tickets {subject, message, category}
    GW->>SUPPORT: Forward (JWT validated)
    SUPPORT->>SUPPORT: Create SupportTicket (status=Open)
    SUPPORT->>MQ: Publish TicketCreated event
    MQ->>NOTIFY: Notify admin of new ticket
    SUPPORT-->>User: Ticket created + ticketId

    Admin->>GW: GET /api/admin/tickets (status=Open)
    GW->>SUPPORT: Forward (Admin JWT)
    SUPPORT-->>Admin: List of open tickets

    Admin->>GW: POST /api/admin/tickets/{id}/reply {message}
    GW->>SUPPORT: Forward
    SUPPORT->>SUPPORT: Create TicketReply
    SUPPORT->>SUPPORT: Update ticket status to Responded
    SUPPORT-->>Admin: Reply saved

    User->>GW: GET /api/support/tickets/{id}
    GW->>SUPPORT: Forward
    SUPPORT-->>User: Ticket + thread of replies
```

---

## 7. Event Bus Topology

All async communication uses RabbitMQ with MassTransit. Each event has one publisher and one or more consumers.

| Event | Publisher | Consumer(s) | Trigger |
|-------|-----------|-------------|---------|
| `UserRegistered` | Auth Service | Rewards Service, Notification Service | New user signup |
| `OtpGenerated` | Auth Service | Notification Service | OTP request |
| `TopUpCompleted` | Wallet Service | Rewards Service, Notification Service | Successful top-up |
| `TransferCompleted` | Wallet Service | Rewards Service, Notification Service | Successful P2P transfer |
| `PaymentFailed` | Wallet Service | Notification Service | Failed payment attempt |
| `KYCApproved` | Admin Service | Auth Service, Notification Service | Admin approves KYC |
| `KYCRejected` | Admin Service | Auth Service, Notification Service | Admin rejects KYC |
| `PointsEarned` | Rewards Service | Notification Service | Points awarded to user |
| `RedemptionCompleted` | Rewards Service | Notification Service | User redeems reward |
| `TicketCreated` | Support Service | Notification Service | New support ticket |

---

## 8. Database-Per-Service Strategy

Each service has its own dedicated SQL Server database. Services never query another service's DB directly — they communicate via APIs or events.

```mermaid
graph LR
    subgraph "AuthServiceDb"
        U["Users"]
        OTP["OTPLogs"]
        RT["RefreshTokens"]
        KYC["KYCDocuments"]
    end

    subgraph "WalletServiceDb"
        WA["WalletAccounts"]
        LE["LedgerEntries"]
        DL["DailyLimitTrackers"]
    end

    subgraph "RewardsServiceDb"
        RA["RewardsAccounts"]
        RTX["RewardsTransactions"]
        RD["Redemptions"]
        ER["EarnRules"]
        RC["RewardsCatalogItems"]
    end

    subgraph "NotificationServiceDb"
        NL["NotificationLogs"]
        NT["NotificationTemplates"]
    end

    subgraph "AdminServiceDb"
        KR["KYCReviews"]
        AL["AdminActivityLogs"]
    end

    subgraph "SupportTicketServiceDb"
        ST["SupportTickets"]
        TR["TicketReplies"]
    end

    AUTH["Auth Service"] --- U
    WALLET["Wallet Service"] --- WA
    REWARDS["Rewards Service"] --- RA
    NOTIFY["Notification Service"] --- NL
    ADMIN["Admin Service"] --- KR
    SUPPORT["Support Service"] --- ST
```

**Schema evolution:** Each service uses EF Core migrations, auto-applied on startup in development. Production deployments should run migrations explicitly before rolling service containers.

---

## 9. Cross-Cutting Concerns

### Authentication & Authorization

```mermaid
flowchart LR
    REQ["Incoming Request"]
    GW_JWT["Gateway JWT\nValidation"]
    SVC_JWT["Service-level\nJWT Middleware"]
    ROLE["Role-Based\nAuthorization\n[Admin] / [User]"]
    HANDLER["Controller\nHandler"]

    REQ --> GW_JWT
    GW_JWT -->|"Invalid token"| REJECT["401 Unauthorized"]
    GW_JWT -->|"Valid"| SVC_JWT
    SVC_JWT --> ROLE
    ROLE -->|"Insufficient role"| FORBIDDEN["403 Forbidden"]
    ROLE -->|"Authorized"| HANDLER
```

- JWT tokens are issued by Auth Service (8-hour validity)
- Refresh tokens allow session extension without re-login
- `UserId` is extracted from JWT claims in all services
- Admin-only endpoints use `[Authorize(Roles = "Admin")]`

### Global Exception Handling

- Each service has a `GlobalExceptionMiddleware` (strategy pattern)
- All unhandled exceptions return a standardized `ApiResponse<T>` JSON:
  ```json
  { "success": false, "message": "Error description", "data": null }
  ```

### Logging

- Serilog is configured in every service
- Logs written to console (dev) and rolling files (`Logs/` directory)
- Structured logging with enriched context (service name, request ID)

### API Response Standard

All endpoints return:
```json
{
  "success": true | false,
  "message": "Human-readable message",
  "data": { ... } | null
}
```

Paginated responses extend with:
```json
{
  "success": true,
  "message": "...",
  "data": {
    "items": [...],
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

---

## 10. Non-Functional Requirements

| Requirement | Approach |
|-------------|---------|
| **Security** | JWT auth, BCrypt hashing, OTP for sensitive ops, HTTPS only |
| **Scalability** | Stateless services (JWT), can scale horizontally behind a load balancer |
| **Isolation** | Database-per-service prevents cascading schema failures |
| **Resilience** | RabbitMQ retries via MassTransit; services degrade gracefully if upstream is down |
| **Observability** | Serilog structured logging; Swagger per service; RabbitMQ management UI |
| **Maintainability** | Clean Architecture in each service; shared DTOs in SharedContracts project |
| **Deployability** | Dockerized multi-stage builds; environment-variable-driven config |
| **Data Consistency** | Eventual consistency via events (not distributed transactions) |
| **Auditability** | AdminActivityLog tracks all admin actions; LedgerEntry is append-only |
