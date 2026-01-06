# MechanicBuddy Management API - Project Structure

## Overview

This is the Management API for the MechanicBuddy SaaS platform. It provides all the functionality needed to manage tenants, handle billing, process demo requests, and administer the platform.

## Directory Structure

```
MechanicBuddy.Management.Api/
├── Authorization/                  # JWT and authorization handlers
│   ├── JwtService.cs              # JWT token generation and validation
│   └── SuperAdminAuthHandler.cs   # Authorization requirement handlers
│
├── Controllers/                    # REST API endpoints
│   ├── AuthController.cs          # Authentication (login, change password)
│   ├── TenantsController.cs       # Tenant management CRUD
│   ├── DemoRequestsController.cs  # Demo request workflow
│   ├── BillingController.cs       # Stripe billing operations
│   ├── DomainsController.cs       # Custom domain verification
│   ├── AnalyticsController.cs     # Platform analytics and metrics
│   └── SuperAdminController.cs    # Super admin management
│
├── Domain/                         # Entity models
│   ├── Tenant.cs                  # Tenant entity
│   ├── DemoRequest.cs             # Demo request entity
│   ├── SuperAdmin.cs              # Super admin user entity
│   ├── DomainVerification.cs      # Domain verification entity
│   ├── TenantMetrics.cs           # Tenant metrics entity
│   └── BillingEvent.cs            # Billing event entity
│
├── Infrastructure/                 # External service clients
│   ├── IKubernetesClient.cs       # Kubernetes client interface
│   ├── KubernetesClient.cs        # K8s client implementation
│   ├── IStripeClient.cs           # Stripe client interface
│   ├── StripeClient.cs            # Stripe integration
│   ├── IEmailClient.cs            # Email client interface
│   └── ResendEmailClient.cs       # Resend email integration
│
├── Repositories/                   # Data access layer
│   ├── ITenantRepository.cs       # Tenant repository interface
│   ├── TenantRepository.cs        # Tenant repository (Dapper)
│   ├── IDemoRequestRepository.cs  # Demo request repository interface
│   ├── DemoRequestRepository.cs   # Demo request repository
│   ├── ISuperAdminRepository.cs   # Super admin repository interface
│   ├── SuperAdminRepository.cs    # Super admin repository
│   ├── IDomainVerificationRepository.cs
│   ├── DomainVerificationRepository.cs
│   ├── ITenantMetricsRepository.cs
│   ├── TenantMetricsRepository.cs
│   ├── IBillingEventRepository.cs
│   └── BillingEventRepository.cs
│
├── Services/                       # Business logic layer
│   ├── TenantService.cs           # Tenant lifecycle management
│   ├── DemoRequestService.cs      # Demo request workflow
│   ├── BillingService.cs          # Stripe billing logic
│   ├── DomainService.cs           # Domain verification logic
│   ├── AnalyticsService.cs        # Analytics and reporting
│   └── SuperAdminService.cs       # Super admin operations
│
├── Properties/
│   └── launchSettings.json        # Development launch configuration
│
├── Program.cs                      # Application entry point and DI setup
├── MechanicBuddy.Management.Api.csproj  # Project file
├── appsettings.json               # Base configuration
├── appsettings.Development.json   # Development configuration
├── appsettings.Secrets.json.example  # Secrets template
├── .gitignore                     # Git ignore rules
└── README.md                      # Project documentation
```

## Key Files Created

### Configuration Files

1. **MechanicBuddy.Management.Api.csproj** - Project file with dependencies:
   - Npgsql & Dapper for database access
   - Stripe.net for billing
   - KubernetesClient for container orchestration
   - BCrypt.Net-Next for password hashing
   - JWT authentication

2. **appsettings.json** - Base configuration
3. **appsettings.Development.json** - Development overrides
4. **appsettings.Secrets.json.example** - Template for secrets
5. **.gitignore** - Excludes secrets and build artifacts

### Domain Entities (6 files)

All entities use proper C# conventions with nullable reference types enabled:

- **Tenant**: Complete tenant information including billing, limits, and Kubernetes details
- **DemoRequest**: Demo account request tracking
- **SuperAdmin**: Platform administrator accounts
- **DomainVerification**: Custom domain verification records
- **TenantMetrics**: Usage metrics and analytics data
- **BillingEvent**: Billing transaction history

### Repository Layer (12 files)

Interfaces and implementations using Dapper for data access:

- **TenantRepository**: Full CRUD + statistics queries
- **DemoRequestRepository**: Demo request management
- **SuperAdminRepository**: Admin user management
- **DomainVerificationRepository**: Domain verification records
- **TenantMetricsRepository**: Metrics storage and aggregation
- **BillingEventRepository**: Billing history and revenue queries

### Service Layer (6 files)

Business logic with proper error handling:

- **TenantService**: Tenant provisioning, K8s orchestration, tier management
- **DemoRequestService**: Demo approval workflow with email notifications
- **BillingService**: Stripe integration, webhook handling
- **DomainService**: DNS/file verification
- **AnalyticsService**: Platform-wide analytics
- **SuperAdminService**: Admin authentication with BCrypt

### Infrastructure Layer (6 files)

External service integrations:

- **KubernetesClient**: Deploy/manage tenant instances in K8s
- **StripeClient**: Customer and subscription management
- **ResendEmailClient**: Transactional email sending

### Authorization Layer (2 files)

- **JwtService**: JWT generation and validation
- **SuperAdminAuthHandler**: Custom authorization policies

### Controllers (7 files)

RESTful API endpoints with Swagger documentation:

- **AuthController**: Login and password management
- **TenantsController**: Tenant CRUD operations
- **DemoRequestsController**: Demo workflow (includes public endpoint)
- **BillingController**: Billing operations and Stripe webhook
- **DomainsController**: Domain verification workflow
- **AnalyticsController**: Platform metrics
- **SuperAdminController**: Admin management

### Entry Point

- **Program.cs**: Complete setup including:
  - JWT authentication
  - Authorization policies
  - CORS configuration
  - Dependency injection
  - Swagger/OpenAPI
  - Health checks

## Database Schema

The API expects these tables in PostgreSQL:

- `tenants` - Tenant records
- `demo_requests` - Demo account requests
- `super_admins` - Platform administrators
- `domain_verifications` - Domain verification records
- `tenant_metrics` - Usage metrics
- `billing_events` - Billing transaction history

(See `../MechanicBuddy.Management.DbUp` for migrations)

## API Endpoints Summary

### Public Endpoints
- `POST /api/demorequests` - Submit demo request
- `POST /api/billing/webhook` - Stripe webhook

### Authenticated Endpoints
- All `/api/auth/*` endpoints (except login)
- All `/api/tenants/*` endpoints
- All `/api/demorequests/*` (admin operations)
- All `/api/billing/*` (except webhook)
- All `/api/domains/*` endpoints
- All `/api/analytics/*` endpoints

### Super Admin Only
- `DELETE /api/tenants/{tenantId}`
- All `/api/superadmin/*` endpoints

## Development Workflow

1. **Start the API**:
   ```bash
   cd /home/damieno/Development/Freelance/MechanicBuddy/backend/src/MechanicBuddy.Management.Api
   dotnet run
   ```

2. **Access Swagger UI**: http://localhost:5100

3. **Login as Admin**:
   ```bash
   curl -X POST http://localhost:5100/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@mechanicbuddy.com","password":"your-password"}'
   ```

4. **Use Token**: Add to Authorization header: `Bearer <token>`

## Configuration Required

Before running, configure these in `appsettings.Secrets.json`:

1. **Database**: PostgreSQL connection string
2. **JWT**: Secret key (minimum 32 characters)
3. **Stripe**: API keys and webhook secret
4. **Email**: Resend API key
5. **Kubernetes**: Cluster configuration (if deploying)

## Next Steps

1. Run database migrations (see `../MechanicBuddy.Management.DbUp`)
2. Create initial super admin account
3. Configure Stripe webhooks
4. Set up Kubernetes cluster (production)
5. Configure DNS for custom domains

## Architecture Decisions

1. **Dapper over EF Core**: Better performance for simple CRUD operations
2. **Repository Pattern**: Separation of data access concerns
3. **Service Layer**: Business logic isolated from controllers
4. **JWT Authentication**: Stateless authentication for scalability
5. **Kubernetes Client**: Direct K8s API integration for tenant isolation
6. **Stripe Webhooks**: Event-driven billing updates
7. **Resend for Email**: Modern, developer-friendly email service

## Security Features

- JWT token-based authentication
- Role-based authorization (admin, super_admin)
- BCrypt password hashing
- CORS configuration
- Secrets excluded from version control
- HTTPS enforcement (production)
- Input validation on all endpoints

## Monitoring & Health

- `/health` - Overall health check
- `/health/ready` - Readiness probe (includes database)
- Structured logging throughout
- Error handling with proper HTTP status codes
