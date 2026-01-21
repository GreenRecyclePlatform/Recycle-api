# 🌱 RecycleHub Platform - Backend API

A comprehensive recycling marketplace platform that connects users with recyclable materials to drivers and suppliers, featuring automated payment processing through PayPal and Stripe.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=flat&logo=dotnet)](https://docs.microsoft.com/en-us/aspnet/core/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)
[![PayPal](https://img.shields.io/badge/PayPal-API-00457C?style=flat&logo=paypal)](https://developer.paypal.com/)
[![Stripe](https://img.shields.io/badge/Stripe-API-008CDD?style=flat&logo=stripe)](https://stripe.com/)

---

## 📋 Table of Contents

- [Business Overview](#-business-overview)
- [Key Features](#-key-features)
- [Technical Architecture](#-technical-architecture)
- [Technology Stack](#-technology-stack)
- [Payment Integration](#-payment-integration)
- [Security Features](#-security-features)
- [Installation & Setup](#-installation--setup)
- [Environment Configuration](#-environment-configuration)
- [Testing](#-testing)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Business Overview

The Recycle Platform is a **circular economy marketplace** that creates a sustainable ecosystem for recyclable materials. The platform generates revenue through a **B2B supplier marketplace** while incentivizing individual users to recycle through automated payments.

### Business Model

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Users     │────────▶│   Platform   │────────▶│  Suppliers  │
│ (Sellers)   │         │              │         │  (Buyers)   │
└─────────────┘         └──────────────┘         └─────────────┘
      │                        │                        │
      │ Sell Recyclables       │ Commission            │ Buy Materials
      │                        │                        │
      ▼                        ▼                        ▼
  💰 PayPal              Platform Revenue          💳 Stripe
  Payouts                                          Payments
```

### Revenue Streams

1. **Commission from Supplier Purchases** - Primary revenue source
2. **Service Fees** - Transaction processing fees
3. **Premium Features** - Advanced analytics and priority pickup (future)

---

## ✨ Key Features

### 👤 User Features
- **Material Submission** - Request pickup for recyclable materials
- **PayPal Integration** - Automated payouts after pickup completion
- **Real-time Tracking** - Monitor request status from submission to payment
- **Payment History** - View all earnings and transactions
- **Driver Reviews** - Rate and review pickup drivers
- **Profile Management** - Update personal information and PayPal email

### 🚛 Driver Features
- **Request Management** - View and accept assigned pickup requests
- **Route Optimization** - Efficient pickup scheduling
- **Status Updates** - Update pickup status in real-time
- **Performance Metrics** - View ratings and completed pickups
- **Earnings Dashboard** - Track driver compensation

### 👨‍💼 Admin Features
- **Request Management** - Approve/reject pickup requests
- **Driver Assignment** - Assign drivers to pickup requests
- **Payment Approval** - Approve user payouts after pickup verification
- **Material Management** - Manage material types and pricing
- **Supplier Orders** - Process supplier material purchases
- **Analytics Dashboard** - Platform-wide metrics and insights

### 🏭 Supplier Features
- **Material Marketplace** - Browse available recycled materials
- **Bulk Ordering** - Purchase materials in bulk quantities
- **Stripe Payment** - Secure payment processing
- **Order History** - Track purchases and invoices
- **Material Analytics** - View availability and pricing trends

---

## 🏗️ Technical Architecture

### Clean Architecture Pattern

```
┌─────────────────────────────────────────────────┐
│                  API Layer                      │
│           (Controllers, Middleware)             │
└─────────────────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────┐
│              Application Layer                  │
│        (Services, DTOs, Interfaces)             │
└─────────────────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────┐
│               Domain Layer                      │
│         (Entities, Enums, Events)               │
└─────────────────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────┐
│            Infrastructure Layer                 │
│    (Data Access, External Services, Auth)       │
└─────────────────────────────────────────────────┘
```
---

## 🛠️ Technology Stack

### Core Technologies
- **.NET 9.0** - Latest LTS version of .NET
- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core 9.0** - ORM for database access
- **SQL Server 2022** - Relational database

### Authentication & Security
- **ASP.NET Core Identity** - User authentication and authorization
- **JWT Bearer Tokens** - Stateless authentication
- **Refresh Tokens** - Secure token refresh mechanism
- **Role-Based Access Control (RBAC)** - Fine-grained permissions

### Payment Processing
- **PayPal Payouts SDK** - User payouts for recyclable materials
- **Stripe API** - Supplier payment processing
- **Sandbox Testing** - Isolated testing environments

### Real-time Communication
- **SignalR** - Real-time notifications and updates
- **WebSocket Support** - Persistent connections for live updates

### External Services
- **Email Service** - Transactional emails and notifications
- **File Storage** - Material images and documents

### Development Tools
- **Swagger/OpenAPI** - API documentation
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging

---

### Base URL
```
[Production: https: https://recycle-web-zj2n.vercel.app/)
Development: https://localhost:7099/api
```

### Authentication
All authenticated endpoints require a JWT Bearer token in the Authorization header:

```http
Authorization: Bearer <your-token-here>
```

---

## 💳 Payment Integration

### PayPal Integration (User Payouts)

The platform uses PayPal Payouts API to automatically pay users for their recyclable materials.

#### Configuration
```json
{
  "PayPal": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Mode": "sandbox" // or "live"
  }
}
```

#### Payout Flow
1. User creates pickup request with PayPal email
2. Driver completes pickup
3. Admin approves payout
4. System sends payout via PayPal API
5. User receives payment in PayPal account

### Stripe Integration (Supplier Payments)

Suppliers pay for materials using Stripe payment processing.

#### Configuration
```json
{
  "Stripe": {
    "SecretKey": "your-secret-key",
    "PublishableKey": "your-publishable-key"
  }
}
```

#### Payment Flow
1. Supplier selects materials to purchase
2. Creates order with Stripe payment intent
3. Completes payment via Stripe checkout
4. System processes order
5. Materials inventory updated


---

## 🔒 Security Features

### Authentication & Authorization
- **JWT Bearer Authentication** - Stateless token-based auth
- **Refresh Token Rotation** - Secure token refresh
- **Role-Based Access Control** - User, Driver, Admin, Supplier roles
- **Password Hashing** - BCrypt password hashing
- **Two-Factor Authentication** - Optional 2FA (future)

### API Security
- **HTTPS Only** - All API calls over TLS
- **CORS Configuration** - Restricted cross-origin requests
- **Rate Limiting** - Request throttling
- **Input Validation** - Comprehensive input sanitization
- **SQL Injection Prevention** - Parameterized queries via EF Core

### Data Protection
- **Personal Data Encryption** - Sensitive data encryption at rest
- **Payment Data Security** - PCI DSS compliance via PayPal/Stripe
- **Audit Logging** - Track all sensitive operations
- **Data Retention Policies** - Automated data cleanup
---

## 🚀 Installation & Setup

### Prerequisites
- .NET 9.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code
- Git

### Step 1: Clone Repository
```bash
git clone https://github.com/your-username/recycle-api.git
cd recycle-api
```

### Step 2: Restore Dependencies
```bash
dotnet restore
```

### Step 3: Configure Database
Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=RecycleDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Step 4: Run Migrations
```bash
dotnet ef database update --project recycle.Infrastructure --startup-project recycle.API
```

### Step 5: Configure External Services

#### PayPal Configuration
1. Create PayPal Developer account
2. Create sandbox app
3. Get Client ID and Secret
4. Update `appsettings.json`:
```json
{
  "PayPal": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Mode": "sandbox"
  }
}
```

#### Stripe Configuration
1. Create Stripe account
2. Get API keys from dashboard
3. Update `appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  }
}
```

### Step 6: Run Application
```bash
dotnet run --project recycle.API
```

The API will be available at:
- HTTPS: `https://localhost:7099`
- Swagger UI: `https://localhost:7099/swagger`
---

## ⚙️ Environment Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "Database": "your-connection-string"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-chars",
    "Issuer": "RecycleAPI",
    "Audience": "RecycleWeb",
    "ExpireMinutes": 60
  },
  "PayPal": {
    "ClientId": "your-paypal-client-id",
    "ClientSecret": "your-paypal-secret",
    "Mode": "sandbox"
  },
  "Stripe": {
    "SecretKey": "your-stripe-secret-key",
    "PublishableKey": "your-stripe-publishable-key"
  },
  "Fireworks": {
    "ApiKey": "your-api-model-key"

}
}
```
**put this inside appsetting.json file in the api project**

### Environment Variables (Production)
```bash
ConnectionStrings__Database=your-connection-string
Jwt__Key=your-jwt-secret
PayPal__ClientId=your-paypal-client-id
PayPal__ClientSecret=your-paypal-secret
Stripe__SecretKey=your-stripe-key
firefly_api model key for the chatbot
```

---

## 🧪 Testing

### API Testing with Swagger
1. Navigate to `https://localhost:7099/swagger`
2. Click "Authorize" button
3. Enter JWT token
4. Test endpoints interactively

---

## 📈 Performance Optimization

### Database Optimization
- Indexed foreign keys and frequently queried columns
- Optimized queries with EF Core includes
- Connection pooling configured
- Query result caching for static data

### API Performance
- Response compression enabled
- Response caching for GET requests
- Asynchronous operations throughout
- Pagination on list endpoints

### Monitoring
- Application Insights integration
- Serilog structured logging
- Performance metrics tracking
- Error tracking and alerts

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation comments
- Write unit tests for new features

### Commit Messages
- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Reference issues and pull requests

---

## 📝 License

This project is licensed under the MIT License 

---

## 👥 Authors
- **Mohammed-Bata** -  [Mohammed-Bata](https://github.com/Mohammed-Bata)
- **Aya Elaidy** -  [Aya-Elaidy](https://github.com/yourusername)
- **Mahitab-Ahmed** -  [Mahitab-Ahmed](https://github.com/MahitabAhmed)
- **MohamedAbdalla23** -  [MohamedAbdalla23](https://github.com/MohamedAbdalla23)
- **Madonna Zakaria** -   [Madonnazakaria](https://github.com/Madonnazakaria)
- **Abdelrahman Helwa** -  [MrHelwa](https://github.com/MrHelwa)
---

**Built with ❤️ by the RecycleHub Platform Team**

---
**MrHelwa is saying Hi🌠**
