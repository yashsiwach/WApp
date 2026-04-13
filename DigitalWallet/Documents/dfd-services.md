# Data Flow Diagrams (DFD) — Digital Wallet Services

> **Note on notation:** Mermaid does not have a native DFD shape set. Each diagram below uses `flowchart TD` with the following conventions:
> - Rounded rectangles `([...])` = **External Entities** (actors outside the system)
> - Rectangles `[...]` = **Processes** (service operations)
> - Cylinders `[("...")]` = **Data Stores** (databases / tables)
> - Labeled arrows = **Data Flows**

## Table of Contents
1. [Auth Service DFD](#1-auth-service-dfd)
2. [Wallet Service DFD](#2-wallet-service-dfd)
3. [Rewards Service DFD](#3-rewards-service-dfd)
4. [Notification Service DFD](#4-notification-service-dfd)
5. [Admin Service DFD](#5-admin-service-dfd)
6. [Support Ticket Service DFD](#6-support-ticket-service-dfd)
7. [Cross-Service Event Flow Summary](#7-cross-service-event-flow-summary)

---

## 1. Auth Service DFD

### 1.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    USER(["👤 End User"])
    ADMIN_ACT(["🛡️ Admin Service"])
    WALLET_ACT(["💰 Wallet Service"])
    MQ_OUT(["🐇 RabbitMQ\n(Event Bus)"])

    AUTH_SYS["AUTH SERVICE\nSystem"]

    USER -->|"Register / Login / OTP / KYC Submit"| AUTH_SYS
    AUTH_SYS -->|"JWT Token / Profile / KYC Status"| USER
    ADMIN_ACT -->|"Update User Status"| AUTH_SYS
    WALLET_ACT -->|"Lookup User by Email"| AUTH_SYS
    AUTH_SYS -->|"UserRegistered / OtpGenerated / KYCSubmitted Events"| MQ_OUT

    style AUTH_SYS fill:#4a90e2,color:#fff
```

### 1.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    USER(["👤 End User"])
    ADMIN_SVC(["🛡️ Admin Service"])
    WALLET_SVC(["💰 Wallet Service"])
    RMQ(["🐇 RabbitMQ"])

    P1["P1: Register User\n(hash password, assign role)"]
    P2["P2: Login\n(verify credentials, issue JWT)"]
    P3["P3: Send OTP\n(generate code, set expiry)"]
    P4["P4: Verify OTP\n(validate code, mark used)"]
    P5["P5: Submit KYC\n(store document, set Pending)"]
    P6["P6: Get Profile\n(fetch user + KYC status)"]
    P7["P7: Update User Status\n(Pending→Active/Suspended)"]
    P8["P8: Lookup User by Email\n(resolve userId)"]
    P9["P9: Refresh Token\n(validate + rotate tokens)"]

    DS_USERS[("🗄️ Users")]
    DS_KYC[("🗄️ KYCDocuments")]
    DS_OTP[("🗄️ OTPLogs")]
    DS_RT[("🗄️ RefreshTokens")]

    USER -->|"name, email, password, phone"| P1
    P1 -->|"write user record"| DS_USERS
    P1 -->|"JWT + userId"| USER
    P1 -->|"UserRegistered event"| RMQ

    USER -->|"email, password"| P2
    P2 -->|"read credentials"| DS_USERS
    P2 -->|"write refresh token"| DS_RT
    P2 -->|"JWT + refreshToken"| USER

    USER -->|"email"| P3
    P3 -->|"write OTP + expiry"| DS_OTP
    P3 -->|"OtpGenerated event"| RMQ
    P3 -->|"OTP sent confirmation"| USER

    USER -->|"email + OTP code"| P4
    P4 -->|"read + validate OTP"| DS_OTP
    P4 -->|"mark OTP used"| DS_OTP
    P4 -->|"success/failure"| USER

    USER -->|"docType, docNumber, imageUrl"| P5
    P5 -->|"write KYC record"| DS_KYC
    P5 -->|"KYCSubmitted event"| RMQ
    P5 -->|"KYC submitted confirmation"| USER

    USER -->|"JWT (userId in claims)"| P6
    P6 -->|"read user"| DS_USERS
    P6 -->|"read KYC status"| DS_KYC
    P6 -->|"profile + KYC status"| USER

    ADMIN_SVC -->|"userId + new status"| P7
    P7 -->|"update user.Status"| DS_USERS
    P7 -->|"200 OK"| ADMIN_SVC

    WALLET_SVC -->|"email"| P8
    P8 -->|"lookup by email"| DS_USERS
    P8 -->|"userId"| WALLET_SVC

    USER -->|"refreshToken"| P9
    P9 -->|"read + validate"| DS_RT
    P9 -->|"revoke old, write new"| DS_RT
    P9 -->|"new JWT + refreshToken"| USER

    style P1 fill:#d4f4d4
    style P2 fill:#d4f4d4
    style P3 fill:#d4f4d4
    style P4 fill:#d4f4d4
    style P5 fill:#d4f4d4
    style P6 fill:#d4f4d4
    style P7 fill:#d4f4d4
    style P8 fill:#d4f4d4
    style P9 fill:#d4f4d4
    style DS_USERS fill:#dce8f5
    style DS_KYC fill:#dce8f5
    style DS_OTP fill:#dce8f5
    style DS_RT fill:#dce8f5
```

### 1.3 KYC Submission Sequence

```mermaid
sequenceDiagram
    participant U as User
    participant AS as Auth Service
    participant KYC_DB as KYCDocuments
    participant MQ as RabbitMQ

    U->>AS: POST /api/kyc/submit {docType, docNumber, imageUrl}
    AS->>AS: Extract userId from JWT
    AS->>KYC_DB: Check for existing submission
    alt Already has Approved KYC
        AS-->>U: 409 Conflict - Already approved
    else
        AS->>KYC_DB: INSERT KYCDocument (status=Pending)
        AS->>MQ: Publish KYCSubmitted {userId, docType, docNumber}
        AS-->>U: 200 OK - KYC submitted
    end
```

---

## 2. Wallet Service DFD

### 2.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    USER(["👤 End User"])
    AUTH_SVC(["🔐 Auth Service"])
    ADMIN_SVC(["🛡️ Admin Service"])
    RMQ(["🐇 RabbitMQ"])

    WALLET_SYS["WALLET SERVICE\nSystem"]

    USER -->|"TopUp / Transfer / Balance / History"| WALLET_SYS
    WALLET_SYS -->|"Balance / Transaction Records / Confirmations"| USER
    AUTH_SVC -->|"UserId lookup response"| WALLET_SYS
    WALLET_SYS -->|"UserId by email request"| AUTH_SVC
    ADMIN_SVC -->|"Create wallet (internal)"| WALLET_SYS
    WALLET_SYS -->|"TopUpCompleted / TransferCompleted / PaymentFailed events"| RMQ

    style WALLET_SYS fill:#27ae60,color:#fff
```

### 2.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    USER(["👤 End User"])
    AUTH_SVC(["🔐 Auth Service"])
    ADMIN_SVC(["🛡️ Admin Service"])
    RMQ(["🐇 RabbitMQ"])

    P1["P1: Create Wallet\n(assign wallet number, set limits)"]
    P2["P2: Top-Up Wallet\n(process payment, credit balance)"]
    P3["P3: Transfer Funds\n(debit sender, credit receiver)"]
    P4["P4: Get Wallet\n(fetch balance + status)"]
    P5["P5: Get Transaction History\n(paginated ledger)"]
    P6["P6: Enforce Daily Limits\n(check + update tracker)"]
    P7["P7: Resolve Recipient\n(email → userId via AuthService)"]

    DS_WALLET[("🗄️ WalletAccounts")]
    DS_LEDGER[("🗄️ LedgerEntries")]
    DS_LIMITS[("🗄️ DailyLimitTrackers")]

    ADMIN_SVC -->|"userId"| P1
    P1 -->|"write wallet record"| DS_WALLET
    P1 -->|"wallet created OK"| ADMIN_SVC

    USER -->|"amount, paymentMethod"| P2
    P2 -->|"read wallet status"| DS_WALLET
    P2 -->|"check + update"| DS_LIMITS
    P2 -->|"update balance"| DS_WALLET
    P2 -->|"write ledger entry TopUp"| DS_LEDGER
    P2 -->|"TopUpCompleted event"| RMQ
    P2 -->|"new balance"| USER

    USER -->|"recipientEmail, amount, note"| P3
    P3 -->|"recipientEmail"| P7
    P7 -->|"GET /user-by-email"| AUTH_SVC
    AUTH_SVC -->|"recipientUserId"| P7
    P7 -->|"recipientUserId"| P3
    P3 -->|"check sender balance"| DS_WALLET
    P3 -->|"check daily limit"| P6
    P6 -->|"read tracker"| DS_LIMITS
    P6 -->|"update tracker"| DS_LIMITS
    P3 -->|"debit sender wallet"| DS_WALLET
    P3 -->|"credit receiver wallet"| DS_WALLET
    P3 -->|"write TransferOut ledger"| DS_LEDGER
    P3 -->|"write TransferIn ledger"| DS_LEDGER
    P3 -->|"TransferCompleted event"| RMQ
    P3 -->|"transfer confirmation"| USER

    USER -->|"JWT userId"| P4
    P4 -->|"read wallet"| DS_WALLET
    P4 -->|"wallet + balance"| USER

    USER -->|"JWT userId, page/pageSize"| P5
    P5 -->|"read ledger"| DS_LEDGER
    P5 -->|"paginated transactions"| USER

    style P1 fill:#d4f8e8
    style P2 fill:#d4f8e8
    style P3 fill:#d4f8e8
    style P4 fill:#d4f8e8
    style P5 fill:#d4f8e8
    style P6 fill:#d4f8e8
    style P7 fill:#d4f8e8
    style DS_WALLET fill:#dce8f5
    style DS_LEDGER fill:#dce8f5
    style DS_LIMITS fill:#dce8f5
```

### 2.3 Transfer Validation Flow

```mermaid
flowchart TD
    START(["Transfer Request"])
    CHECK_WALLET["Check wallet exists\n& status = Active"]
    CHECK_BAL["Check balance >= amount"]
    CHECK_LIMIT["Check daily limit\nnot exceeded"]
    RESOLVE["Resolve recipient\nby email"]
    CHECK_SELF["Ensure not\nself-transfer"]
    DEBIT["Debit sender\n+ write LedgerEntry"]
    CREDIT["Credit recipient\n+ write LedgerEntry"]
    PUBLISH["Publish\nTransferCompleted event"]
    SUCCESS(["Transfer Complete"])
    FAIL_W(["400 Wallet Inactive"])
    FAIL_B(["400 Insufficient Balance"])
    FAIL_L(["400 Daily Limit Exceeded"])
    FAIL_R(["404 Recipient Not Found"])
    FAIL_S(["400 Cannot Transfer to Self"])

    START --> CHECK_WALLET
    CHECK_WALLET -->|"Not found / Locked"| FAIL_W
    CHECK_WALLET -->|"Active"| CHECK_BAL
    CHECK_BAL -->|"Insufficient"| FAIL_B
    CHECK_BAL -->|"OK"| CHECK_LIMIT
    CHECK_LIMIT -->|"Exceeded"| FAIL_L
    CHECK_LIMIT -->|"Within limit"| RESOLVE
    RESOLVE -->|"Not found"| FAIL_R
    RESOLVE -->|"Found"| CHECK_SELF
    CHECK_SELF -->|"Same user"| FAIL_S
    CHECK_SELF -->|"Different user"| DEBIT
    DEBIT --> CREDIT
    CREDIT --> PUBLISH
    PUBLISH --> SUCCESS
```

---

## 3. Rewards Service DFD

### 3.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    USER(["👤 End User"])
    WALLET_SVC(["💰 Wallet Service\n(internal HTTP)"])
    RMQ_IN(["🐇 RabbitMQ\n(Events IN)"])
    RMQ_OUT(["🐇 RabbitMQ\n(Events OUT)"])

    REWARDS_SYS["REWARDS SERVICE\nSystem"]

    USER -->|"View points / Redeem / Browse catalog"| REWARDS_SYS
    REWARDS_SYS -->|"Points balance / Tier / History / Catalog"| USER
    WALLET_SVC -->|"POST /reward/award (internal)"| REWARDS_SYS
    RMQ_IN -->|"UserRegistered / TopUpCompleted / TransferCompleted"| REWARDS_SYS
    REWARDS_SYS -->|"PointsEarned / RedemptionCompleted"| RMQ_OUT

    style REWARDS_SYS fill:#8e44ad,color:#fff
```

### 3.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    USER(["👤 End User"])
    ADMIN(["🛡️ Admin"])
    RMQ_IN(["🐇 RabbitMQ IN"])
    RMQ_OUT(["🐇 RabbitMQ OUT"])

    P1["P1: Initialize Rewards Account\n(on UserRegistered event)"]
    P2["P2: Award Points\n(calculate from event/request)"]
    P3["P3: Update Tier\n(check cumulative points vs thresholds)"]
    P4["P4: Get Rewards Summary\n(balance + tier + progress)"]
    P5["P5: Browse Catalog\n(list active items)"]
    P6["P6: Redeem Points\n(deduct points, create redemption)"]
    P7["P7: Manage Catalog\n(CRUD catalog items)"]
    P8["P8: Get History\n(paginated transactions)"]

    DS_ACCOUNTS[("🗄️ RewardsAccounts")]
    DS_TXNS[("🗄️ RewardsTransactions")]
    DS_CATALOG[("🗄️ RewardsCatalogItems")]
    DS_RULES[("🗄️ EarnRules")]
    DS_REDEEM[("🗄️ Redemptions")]

    RMQ_IN -->|"UserRegistered {userId}"| P1
    P1 -->|"create account (0 pts, Bronze)"| DS_ACCOUNTS

    RMQ_IN -->|"TopUpCompleted / TransferCompleted events"| P2
    P2 -->|"read earn rules"| DS_RULES
    P2 -->|"calculate points"| P2
    P2 -->|"update points balance"| DS_ACCOUNTS
    P2 -->|"write RewardsTransaction"| DS_TXNS
    P2 -->|"trigger"| P3
    P3 -->|"read total earned"| DS_ACCOUNTS
    P3 -->|"update tier if threshold crossed"| DS_ACCOUNTS
    P2 -->|"PointsEarned event"| RMQ_OUT

    USER -->|"JWT userId"| P4
    P4 -->|"read account"| DS_ACCOUNTS
    P4 -->|"points + tier + next tier progress"| USER

    USER -->|"JWT userId"| P5
    P5 -->|"read active items"| DS_CATALOG
    P5 -->|"catalog list"| USER

    USER -->|"catalogItemId"| P6
    P6 -->|"read catalog item"| DS_CATALOG
    P6 -->|"check sufficient points"| DS_ACCOUNTS
    P6 -->|"deduct points"| DS_ACCOUNTS
    P6 -->|"decrement stock"| DS_CATALOG
    P6 -->|"write Redemption"| DS_REDEEM
    P6 -->|"write RewardsTransaction (Redeem)"| DS_TXNS
    P6 -->|"RedemptionCompleted event"| RMQ_OUT
    P6 -->|"redemption confirmation"| USER

    ADMIN -->|"item details"| P7
    P7 -->|"write/update catalog"| DS_CATALOG
    P7 -->|"item created/updated"| ADMIN

    USER -->|"page/pageSize"| P8
    P8 -->|"read transactions"| DS_TXNS
    P8 -->|"paginated history"| USER

    style P1 fill:#f5e6ff
    style P2 fill:#f5e6ff
    style P3 fill:#f5e6ff
    style P4 fill:#f5e6ff
    style P5 fill:#f5e6ff
    style P6 fill:#f5e6ff
    style P7 fill:#f5e6ff
    style P8 fill:#f5e6ff
```

### 3.3 Tier Progression Flow

```mermaid
flowchart LR
    EARN["Points Awarded"]
    CALC["Calculate\nTotal Earned\n(cumulative)"]
    B{"≥ 1,000?"}
    S{"≥ 5,000?"}
    G{"≥ 15,000?"}
    BRONZE["🥉 Bronze\n0–999 pts"]
    SILVER["🥈 Silver\n1,000–4,999 pts"]
    GOLD["🥇 Gold\n5,000–14,999 pts"]
    PLATINUM["💎 Platinum\n15,000+ pts"]

    EARN --> CALC
    CALC --> B
    B -->|"No"| BRONZE
    B -->|"Yes"| S
    S -->|"No"| SILVER
    S -->|"Yes"| G
    G -->|"No"| GOLD
    G -->|"Yes"| PLATINUM
```

---

## 4. Notification Service DFD

### 4.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    USER(["👤 End User"])
    RMQ_IN(["🐇 RabbitMQ\n(All system events)"])

    NOTIFY_SYS["NOTIFICATION SERVICE\nSystem"]

    RMQ_IN -->|"All system events (10+ types)"| NOTIFY_SYS
    NOTIFY_SYS -->|"Store + deliver notifications"| NOTIFY_SYS
    USER -->|"Get notifications / Mark read"| NOTIFY_SYS
    NOTIFY_SYS -->|"Notification list / Unread count"| USER

    style NOTIFY_SYS fill:#e67e22,color:#fff
```

### 4.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    USER(["👤 End User"])
    RMQ(["🐇 RabbitMQ\n(event consumers)"])

    P1["P1: Consume Event\n(receive from RabbitMQ)"]
    P2["P2: Resolve Template\n(lookup template by event type)"]
    P3["P3: Render Message\n(interpolate template with event data)"]
    P4["P4: Store Notification\n(persist to DB, IsRead=false)"]
    P5["P5: Get Notifications\n(paginated, newest first)"]
    P6["P6: Get Unread Count\n(count IsRead=false)"]
    P7["P7: Mark as Read\n(set IsRead=true + ReadAt)"]
    P8["P8: Mark All Read\n(bulk update for userId)"]

    DS_LOGS[("🗄️ NotificationLogs")]
    DS_TMPL[("🗄️ NotificationTemplates")]

    RMQ -->|"Event payload (userId, event data)"| P1
    P1 --> P2
    P2 -->|"read template by eventType"| DS_TMPL
    P2 -->|"template (title, message)"| P3
    P3 -->|"rendered title + message"| P4
    P4 -->|"write NotificationLog"| DS_LOGS

    USER -->|"JWT userId, page"| P5
    P5 -->|"read logs for userId"| DS_LOGS
    P5 -->|"paginated notifications"| USER

    USER -->|"JWT userId"| P6
    P6 -->|"COUNT where IsRead=false"| DS_LOGS
    P6 -->|"unread count"| USER

    USER -->|"notificationId"| P7
    P7 -->|"update IsRead=true"| DS_LOGS
    P7 -->|"200 OK"| USER

    USER -->|"JWT userId"| P8
    P8 -->|"bulk update IsRead=true"| DS_LOGS
    P8 -->|"200 OK"| USER

    style P1 fill:#fde8cc
    style P2 fill:#fde8cc
    style P3 fill:#fde8cc
    style P4 fill:#fde8cc
    style P5 fill:#fde8cc
    style P6 fill:#fde8cc
    style P7 fill:#fde8cc
    style P8 fill:#fde8cc
```

### 4.3 Event-to-Notification Mapping

```mermaid
flowchart LR
    subgraph "Incoming Events"
        E1["UserRegistered"]
        E2["OtpGenerated"]
        E3["TopUpCompleted"]
        E4["TransferCompleted\n(sender)"]
        E5["TransferCompleted\n(recipient)"]
        E6["KYCApproved"]
        E7["KYCRejected"]
        E8["PointsEarned"]
        E9["RedemptionCompleted"]
        E10["TicketCreated"]
        E11["PaymentFailed"]
    end

    subgraph "Notification Messages"
        N1["🎉 Welcome to Digital Wallet!"]
        N2["🔐 Your OTP is: XXXXXX"]
        N3["✅ Wallet topped up: ₹X.XX"]
        N4["📤 Transfer sent: ₹X.XX"]
        N5["📥 Transfer received: ₹X.XX"]
        N6["✅ KYC Approved — Wallet activated!"]
        N7["❌ KYC Rejected — Reason: ..."]
        N8["🏆 X points earned!"]
        N9["🎁 Redemption confirmed: Item"]
        N10["🎫 Ticket #XXX created"]
        N11["⚠️ Payment of ₹X failed"]
    end

    E1 --> N1
    E2 --> N2
    E3 --> N3
    E4 --> N4
    E5 --> N5
    E6 --> N6
    E7 --> N7
    E8 --> N8
    E9 --> N9
    E10 --> N10
    E11 --> N11
```

---

## 5. Admin Service DFD

### 5.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    ADMIN_USER(["🛡️ Admin User"])
    AUTH_SVC(["🔐 Auth Service"])
    WALLET_SVC(["💰 Wallet Service"])
    RMQ_IN(["🐇 RabbitMQ IN\n(KYCSubmitted)"])
    RMQ_OUT(["🐇 RabbitMQ OUT\n(KYCApproved/Rejected)"])

    ADMIN_SYS["ADMIN SERVICE\nSystem"]

    ADMIN_USER -->|"Review KYC / Dashboard / Manage Catalog"| ADMIN_SYS
    ADMIN_SYS -->|"KYC queue / Metrics / Catalog items"| ADMIN_USER
    RMQ_IN -->|"KYCSubmitted event"| ADMIN_SYS
    ADMIN_SYS -->|"Update user status"| AUTH_SVC
    ADMIN_SYS -->|"Create wallet"| WALLET_SVC
    ADMIN_SYS -->|"KYCApproved / KYCRejected events"| RMQ_OUT

    style ADMIN_SYS fill:#c0392b,color:#fff
```

### 5.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    ADMIN(["🛡️ Admin"])
    AUTH_SVC(["🔐 Auth Service"])
    WALLET_SVC(["💰 Wallet Service"])
    RMQ_IN(["🐇 RabbitMQ IN"])
    RMQ_OUT(["🐇 RabbitMQ OUT"])

    P1["P1: Sync KYC Submission\n(consume KYCSubmitted event)"]
    P2["P2: List KYC Reviews\n(pending / all with filters)"]
    P3["P3: Approve KYC\n(update status, create wallet, notify)"]
    P4["P4: Reject KYC\n(update status, store reason, notify)"]
    P5["P5: Get Dashboard Metrics\n(counts, stats aggregation)"]
    P6["P6: Log Admin Activity\n(audit trail)"]

    DS_KYC[("🗄️ KYCReviews")]
    DS_LOG[("🗄️ AdminActivityLogs")]

    RMQ_IN -->|"KYCSubmitted {userId, docType, email}"| P1
    P1 -->|"write KYCReview (status=Pending)"| DS_KYC

    ADMIN -->|"filter: status, priority"| P2
    P2 -->|"read KYCReviews"| DS_KYC
    P2 -->|"KYC review list"| ADMIN

    ADMIN -->|"userId + adminNote"| P3
    P3 -->|"update KYCReview status=Approved"| DS_KYC
    P3 -->|"PUT /api/auth/update-status (Active)"| AUTH_SVC
    AUTH_SVC -->|"200 OK"| P3
    P3 -->|"POST /api/wallet/create-internal"| WALLET_SVC
    WALLET_SVC -->|"wallet created"| P3
    P3 -->|"KYCApproved event"| RMQ_OUT
    P3 -->|"trigger"| P6
    P6 -->|"write audit log"| DS_LOG
    P3 -->|"approval confirmation"| ADMIN

    ADMIN -->|"userId + rejectionReason"| P4
    P4 -->|"update KYCReview status=Rejected"| DS_KYC
    P4 -->|"PUT /api/auth/update-status (Rejected)"| AUTH_SVC
    P4 -->|"KYCRejected event"| RMQ_OUT
    P4 -->|"trigger"| P6
    P4 -->|"rejection confirmation"| ADMIN

    ADMIN -->|"dashboard request"| P5
    P5 -->|"aggregate KYC counts"| DS_KYC
    P5 -->|"metrics + stats"| ADMIN

    style P1 fill:#fde8e8
    style P2 fill:#fde8e8
    style P3 fill:#fde8e8
    style P4 fill:#fde8e8
    style P5 fill:#fde8e8
    style P6 fill:#fde8e8
```

### 5.3 KYC Approval Workflow

```mermaid
sequenceDiagram
    actor A as Admin
    participant AS as Admin Service
    participant KYC_DB as KYCReviews DB
    participant AUTH as Auth Service
    participant WALLET as Wallet Service
    participant MQ as RabbitMQ
    participant NOTIFY as Notification Service

    A->>AS: GET /api/admin/kyc/pending
    AS->>KYC_DB: SELECT WHERE status='Pending'
    AS-->>A: List of pending KYC reviews

    A->>AS: PUT /api/admin/kyc/{userId}/approve {note}
    AS->>KYC_DB: UPDATE status='Approved', reviewedBy, reviewedAt
    AS->>AUTH: PUT /api/auth/update-status {userId, status:'Active'}
    AUTH-->>AS: 200 OK
    AS->>WALLET: POST /api/wallet/create-internal {userId}
    WALLET-->>AS: WalletAccount created
    AS->>MQ: Publish KYCApproved {userId, email}
    MQ->>NOTIFY: Store "KYC Approved" notification for user
    AS-->>A: 200 OK - KYC approved
```

---

## 6. Support Ticket Service DFD

### 6.1 Context Diagram (Level 0)

```mermaid
flowchart TD
    USER(["👤 End User"])
    ADMIN_USER(["🛡️ Admin User"])
    RMQ_OUT(["🐇 RabbitMQ OUT\n(TicketCreated)"])

    SUPPORT_SYS["SUPPORT TICKET\nSERVICE System"]

    USER -->|"Create ticket / Add reply / View tickets"| SUPPORT_SYS
    SUPPORT_SYS -->|"Ticket details / Thread / Status"| USER
    ADMIN_USER -->|"View tickets / Reply / Assign / Close"| SUPPORT_SYS
    SUPPORT_SYS -->|"Ticket list / Thread"| ADMIN_USER
    SUPPORT_SYS -->|"TicketCreated event"| RMQ_OUT

    style SUPPORT_SYS fill:#16a085,color:#fff
```

### 6.2 Detailed DFD (Level 1)

```mermaid
flowchart TD
    USER(["👤 End User"])
    ADMIN(["🛡️ Admin"])
    RMQ(["🐇 RabbitMQ"])

    P1["P1: Create Ticket\n(generate ticket number, set Open)"]
    P2["P2: List User Tickets\n(get own tickets, paginated)"]
    P3["P3: Get Ticket Detail\n(ticket + reply thread)"]
    P4["P4: Add User Reply\n(append message to thread)"]
    P5["P5: List All Tickets\n(admin: filter by status/category)"]
    P6["P6: Admin Reply\n(append admin message, set Responded)"]
    P7["P7: Assign Ticket\n(set assignedToAdminId)"]
    P8["P8: Close Ticket\n(set status=Closed)"]
    P9["P9: Generate Ticket Number\n(unique: TKT-YYYYMMDD-XXXX)"]

    DS_TICKETS[("🗄️ SupportTickets")]
    DS_REPLIES[("🗄️ TicketReplies")]

    USER -->|"subject, message, category"| P1
    P1 -->|"generate number"| P9
    P9 -->|"unique ticketNumber"| P1
    P1 -->|"write SupportTicket"| DS_TICKETS
    P1 -->|"TicketCreated event"| RMQ
    P1 -->|"ticket created + ticketId"| USER

    USER -->|"JWT userId, page"| P2
    P2 -->|"read tickets WHERE userId"| DS_TICKETS
    P2 -->|"own ticket list"| USER

    USER -->|"ticketId"| P3
    P3 -->|"read ticket"| DS_TICKETS
    P3 -->|"read replies"| DS_REPLIES
    P3 -->|"ticket + thread"| USER

    USER -->|"ticketId + message"| P4
    P4 -->|"write TicketReply (isAdmin=false)"| DS_REPLIES
    P4 -->|"update ticket.Status=InProgress"| DS_TICKETS
    P4 -->|"200 OK"| USER

    ADMIN -->|"status/category filters, page"| P5
    P5 -->|"read all tickets with filters"| DS_TICKETS
    P5 -->|"ticket list"| ADMIN

    ADMIN -->|"ticketId + message"| P6
    P6 -->|"write TicketReply (isAdmin=true)"| DS_REPLIES
    P6 -->|"update ticket.Status=Responded"| DS_TICKETS
    P6 -->|"200 OK"| ADMIN

    ADMIN -->|"ticketId + adminId"| P7
    P7 -->|"update assignedToAdminId"| DS_TICKETS
    P7 -->|"200 OK"| ADMIN

    ADMIN -->|"ticketId"| P8
    P8 -->|"update status=Closed"| DS_TICKETS
    P8 -->|"200 OK"| ADMIN

    style P1 fill:#d4f0ec
    style P2 fill:#d4f0ec
    style P3 fill:#d4f0ec
    style P4 fill:#d4f0ec
    style P5 fill:#d4f0ec
    style P6 fill:#d4f0ec
    style P7 fill:#d4f0ec
    style P8 fill:#d4f0ec
    style P9 fill:#d4f0ec
```

### 6.3 Ticket Lifecycle State Machine

```mermaid
stateDiagram-v2
    [*] --> Open : User creates ticket

    Open --> InProgress : User adds reply
    Open --> Responded : Admin replies
    Open --> Closed : Admin closes directly

    InProgress --> Responded : Admin replies
    InProgress --> Closed : Admin closes

    Responded --> InProgress : User replies again
    Responded --> Closed : Admin closes

    Closed --> [*]
```

---

## 7. Cross-Service Event Flow Summary

Complete picture of how all services interact through events:

```mermaid
flowchart LR
    subgraph "Event Publishers"
        AUTH_PUB["Auth Service\nPublishes:\n• UserRegistered\n• OtpGenerated\n• KYCSubmitted"]
        WALLET_PUB["Wallet Service\nPublishes:\n• TopUpCompleted\n• TransferCompleted\n• PaymentFailed"]
        ADMIN_PUB["Admin Service\nPublishes:\n• KYCApproved\n• KYCRejected"]
        REWARD_PUB["Rewards Service\nPublishes:\n• PointsEarned\n• RedemptionCompleted"]
        SUPPORT_PUB["Support Service\nPublishes:\n• TicketCreated"]
    end

    RMQ[("🐇 RabbitMQ\nMessage Bus")]

    subgraph "Event Consumers"
        REWARD_CON["Rewards Service\nConsumes:\n• UserRegistered\n• TopUpCompleted\n• TransferCompleted"]
        NOTIFY_CON["Notification Service\nConsumes:\n• ALL events"]
        AUTH_CON["Auth Service\nConsumes:\n• KYCApproved\n• KYCRejected"]
        ADMIN_CON["Admin Service\nConsumes:\n• KYCSubmitted"]
    end

    AUTH_PUB --> RMQ
    WALLET_PUB --> RMQ
    ADMIN_PUB --> RMQ
    REWARD_PUB --> RMQ
    SUPPORT_PUB --> RMQ

    RMQ --> REWARD_CON
    RMQ --> NOTIFY_CON
    RMQ --> AUTH_CON
    RMQ --> ADMIN_CON

    style RMQ fill:#ff6b35,color:#fff
```
