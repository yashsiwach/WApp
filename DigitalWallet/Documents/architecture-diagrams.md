# Architecture Diagrams — Digital Wallet

## Table of Contents
1. [System Overview](#1-system-overview)
2. [Microservices Topology](#2-microservices-topology)
3. [Network & Deployment Architecture](#3-network--deployment-architecture)
4. [Synchronous Communication Map](#4-synchronous-communication-map)
5. [Asynchronous Event Bus Topology](#5-asynchronous-event-bus-topology)
6. [Technology Stack](#6-technology-stack)

---

## 1. System Overview

High-level C4-style container view: how a client interacts with the system and what major building blocks exist.

```mermaid
graph TD
    Client(["👤 Client\n(Mobile / Web App)"])

    subgraph Gateway Layer
        GW["🔀 API Gateway\n(Ocelot · Port 5000)"]
    end

    subgraph Microservices
        AUTH["🔐 Auth Service\nRegistration · Login · KYC · OTP"]
        WALLET["💰 Wallet Service\nBalance · Transfer · Top-Up · Ledger"]
        REWARD["🏆 Rewards Service\nPoints · Tiers · Redemptions"]
        NOTIFY["🔔 Notification Service\nAlerts · Templates · Logs"]
        ADMIN["🛡️ Admin Service\nKYC Review · Dashboard · Catalog"]
        SUPPORT["🎫 Support Ticket Service\nTickets · Replies · Resolution"]
    end

    subgraph Message Broker
        RMQ[("🐇 RabbitMQ\nEvent Bus")]
    end

    subgraph Databases
        DB_AUTH[("🗄️ AuthServiceDb\nSQL Server")]
        DB_WALLET[("🗄️ WalletServiceDb\nSQL Server")]
        DB_REWARD[("🗄️ RewardsServiceDb\nSQL Server")]
        DB_NOTIFY[("🗄️ NotificationServiceDb\nSQL Server")]
        DB_ADMIN[("🗄️ AdminServiceDb\nSQL Server")]
        DB_SUPPORT[("🗄️ SupportTicketServiceDb\nSQL Server")]
    end

    Client -->|"HTTPS REST"| GW
    GW -->|"Route /api/auth/*"| AUTH
    GW -->|"Route /api/wallet/*"| WALLET
    GW -->|"Route /api/reward/*"| REWARD
    GW -->|"Route /api/notifications/*"| NOTIFY
    GW -->|"Route /api/admin/*"| ADMIN
    GW -->|"Route /api/support/*"| SUPPORT

    AUTH --- DB_AUTH
    WALLET --- DB_WALLET
    REWARD --- DB_REWARD
    NOTIFY --- DB_NOTIFY
    ADMIN --- DB_ADMIN
    SUPPORT --- DB_SUPPORT

    AUTH -->|"Publishes events"| RMQ
    WALLET -->|"Publishes events"| RMQ
    ADMIN -->|"Publishes events"| RMQ
    SUPPORT -->|"Publishes events"| RMQ
    RMQ -->|"Consumes events"| REWARD
    RMQ -->|"Consumes events"| NOTIFY
    RMQ -->|"Consumes events"| ADMIN

    style GW fill:#f0a500,color:#000
    style RMQ fill:#ff6b35,color:#fff
    style Client fill:#4a90e2,color:#fff
```

---

## 2. Microservices Topology

Each service is an independently deployable ASP.NET Core 8 Web API.

```mermaid
graph LR
    subgraph "Port 5000 — Entry Point"
        GW["API Gateway\n(Ocelot)"]
    end

    subgraph "Port 5001 — Auth Domain"
        A1["AuthController\n/api/auth"]
        A2["OTPController\n/api/otp"]
        A3["KYCController\n/api/kyc"]
    end

    subgraph "Port 5002 — Wallet Domain"
        W1["WalletController\n/api/wallet"]
        W2["AdminWalletController\n/api/admin/wallet"]
    end

    subgraph "Port 5003 — Rewards Domain"
        R1["RewardsController\n/api/reward"]
    end

    subgraph "Port 5004 — Notification Domain"
        N1["NotificationsController\n/api/notifications"]
    end

    subgraph "Port 5005 — Admin Domain"
        AD1["KYCController\n/api/admin/kyc"]
        AD2["DashboardController\n/api/admin/dashboard"]
        AD3["RewardsCatalogController\n/api/admin/catalog"]
    end

    subgraph "Port 5006 — Support Domain"
        S1["UserTicketsController\n/api/support/tickets"]
        S2["AdminTicketsController\n/api/admin/tickets"]
    end

    GW --> A1
    GW --> A2
    GW --> A3
    GW --> W1
    GW --> R1
    GW --> N1
    GW --> AD1
    GW --> AD2
    GW --> AD3
    GW --> S1
    GW --> S2
```

---

## 3. Network & Deployment Architecture

```mermaid
graph TD
    Internet(["🌐 Internet"])

    subgraph "Docker Host / Cloud VM"
        subgraph "Gateway Container"
            GW["Ocelot API Gateway\n:5000"]
        end

        subgraph "Service Containers"
            SVC1["Auth Service\n:5001"]
            SVC2["Wallet Service\n:5002"]
            SVC3["Rewards Service\n:5003"]
            SVC4["Notification Service\n:5004"]
            SVC5["Admin Service\n:5005"]
            SVC6["Support Ticket Service\n:5006"]
        end

        subgraph "Infrastructure Containers"
            SQLSERVER[("SQL Server\nMultiple DBs")]
            RABBITMQ["RabbitMQ\n:5672 (AMQP)\n:15672 (Mgmt)"]
        end
    end

    Internet -->|"Port 5000 only exposed"| GW
    GW --> SVC1
    GW --> SVC2
    GW --> SVC3
    GW --> SVC4
    GW --> SVC5
    GW --> SVC6

    SVC1 <-->|"TCP 1433"| SQLSERVER
    SVC2 <-->|"TCP 1433"| SQLSERVER
    SVC3 <-->|"TCP 1433"| SQLSERVER
    SVC4 <-->|"TCP 1433"| SQLSERVER
    SVC5 <-->|"TCP 1433"| SQLSERVER
    SVC6 <-->|"TCP 1433"| SQLSERVER

    SVC1 <-->|"AMQP 5672"| RABBITMQ
    SVC2 <-->|"AMQP 5672"| RABBITMQ
    SVC3 <-->|"AMQP 5672"| RABBITMQ
    SVC4 <-->|"AMQP 5672"| RABBITMQ
    SVC5 <-->|"AMQP 5672"| RABBITMQ
    SVC6 <-->|"AMQP 5672"| RABBITMQ

    style Internet fill:#4a90e2,color:#fff
    style GW fill:#f0a500,color:#000
    style RABBITMQ fill:#ff6b35,color:#fff
    style SQLSERVER fill:#3a7ebf,color:#fff
```

---

## 4. Synchronous Communication Map

Services that call each other directly over HTTP (internal network only — never through the gateway).

```mermaid
graph LR
    WALLET["Wallet Service"]
    AUTH["Auth Service"]
    ADMIN["Admin Service"]
    REWARD["Rewards Service"]

    WALLET -->|"GET /api/auth/user-by-email\n(resolve userId from email)"| AUTH
    WALLET -->|"POST /api/reward/award\n(award points after transfer/topup)"| REWARD
    ADMIN -->|"PUT /api/auth/update-status\n(activate user after KYC approval)"| AUTH
    ADMIN -->|"POST /api/wallet/create-internal\n(create wallet after KYC approval)"| WALLET

    style AUTH fill:#e8f4f8
    style WALLET fill:#e8f8e8
    style ADMIN fill:#f8f0e8
    style REWARD fill:#f8e8f8
```

---

## 5. Asynchronous Event Bus Topology

All events flow through RabbitMQ (MassTransit). Publishers and consumers are decoupled.

```mermaid
graph TD
    subgraph Publishers
        AUTH_P["Auth Service"]
        WALLET_P["Wallet Service"]
        ADMIN_P["Admin Service"]
        SUPPORT_P["Support Ticket Service"]
    end

    subgraph "RabbitMQ Exchange / Queues"
        E1(["UserRegistered"])
        E2(["TopUpCompleted"])
        E3(["TransferCompleted"])
        E4(["KYCApproved"])
        E5(["KYCRejected"])
        E6(["PointsEarned"])
        E7(["RedemptionCompleted"])
        E8(["TicketCreated"])
        E9(["OtpGenerated"])
        E10(["PaymentFailed"])
    end

    subgraph Consumers
        REWARD_C["Rewards Service\nConsumers"]
        NOTIFY_C["Notification Service\nConsumers"]
        ADMIN_C["Admin Service\nConsumers"]
        AUTH_C["Auth Service\nConsumers"]
    end

    AUTH_P --> E1
    AUTH_P --> E9
    WALLET_P --> E2
    WALLET_P --> E3
    WALLET_P --> E10
    ADMIN_P --> E4
    ADMIN_P --> E5
    REWARD_C --> E6
    REWARD_C --> E7
    SUPPORT_P --> E8

    E1 --> REWARD_C
    E1 --> NOTIFY_C
    E2 --> REWARD_C
    E2 --> NOTIFY_C
    E3 --> REWARD_C
    E3 --> NOTIFY_C
    E4 --> AUTH_C
    E4 --> NOTIFY_C
    E5 --> AUTH_C
    E5 --> NOTIFY_C
    E6 --> NOTIFY_C
    E7 --> NOTIFY_C
    E8 --> NOTIFY_C
    E9 --> NOTIFY_C
    E10 --> NOTIFY_C

    style E1 fill:#d4f1f4
    style E2 fill:#d4f1f4
    style E3 fill:#d4f1f4
    style E4 fill:#d4f4d4
    style E5 fill:#f4d4d4
    style E6 fill:#f4f4d4
    style E7 fill:#f4f4d4
    style E8 fill:#f4e8d4
    style E9 fill:#d4f1f4
    style E10 fill:#f4d4d4
```

---

## 6. Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Runtime** | .NET 8 / C# | All microservices backend |
| **Web Framework** | ASP.NET Core 8 Web API | REST API hosting |
| **API Gateway** | Ocelot | Request routing, JWT validation |
| **ORM** | Entity Framework Core 8 | SQL Server data access |
| **Database** | SQL Server (multi-instance) | Persistent storage per service |
| **Message Broker** | RabbitMQ | Async event bus |
| **Messaging Library** | MassTransit 8.2 | RabbitMQ abstraction, consumers |
| **Authentication** | JWT Bearer Tokens | Stateless auth with refresh tokens |
| **Password Hashing** | BCrypt | Secure password storage |
| **OTP** | TOTP / numeric OTP | Two-factor auth support |
| **Logging** | Serilog | Structured logging to console + files |
| **API Docs** | Swagger / Swashbuckle 6.8 | OpenAPI spec per service |
| **Containerization** | Docker (multi-stage) | Service packaging & deployment |
| **Migrations** | EF Core Migrations | DB schema version control |
| **Shared Contracts** | SharedContracts project | DTOs, enums, events across services |

```mermaid
graph LR
    subgraph "Client Layer"
        C["REST Client\n(Mobile / Web)"]
    end

    subgraph "Edge Layer"
        GW["Ocelot Gateway\nJWT Validation\nRouting\nCORS"]
    end

    subgraph "Application Layer"
        AS[".NET 8\nASP.NET Core\nWeb API\nx6 Services"]
    end

    subgraph "Messaging Layer"
        MT["MassTransit\n+ RabbitMQ"]
    end

    subgraph "Persistence Layer"
        EF["EF Core 8\n+ Migrations"]
        SQL["SQL Server\n6 Databases"]
    end

    subgraph "Cross-Cutting"
        LOG["Serilog\nLogging"]
        JWT["JWT\nAuth"]
        SW["Swagger\nDocs"]
        EX["Global Exception\nMiddleware"]
    end

    C -->|"HTTPS"| GW
    GW --> AS
    AS <--> MT
    AS --> EF
    EF --> SQL
    AS -.-> LOG
    AS -.-> JWT
    AS -.-> SW
    AS -.-> EX

    style GW fill:#f0a500,color:#000
    style MT fill:#ff6b35,color:#fff
    style SQL fill:#3a7ebf,color:#fff
```
