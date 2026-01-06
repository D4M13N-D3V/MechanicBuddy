# MechanicBuddy Management API

The Management API is the control plane for the MechanicBuddy SaaS platform. It handles tenant provisioning, billing, domain verification, and platform administration.

## Features

- **Tenant Management**: Create, update, suspend, and delete tenant instances
- **Demo Requests**: Handle demo account requests and approvals
- **Billing**: Stripe integration for subscription management
- **Domain Verification**: Custom domain verification (DNS and file-based)
- **Analytics**: Platform-wide metrics and tenant analytics
- **Super Admin**: Authentication and authorization for platform administrators

## Architecture

### Layers

1. **Controllers**: REST API endpoints
2. **Services**: Business logic and orchestration
3. **Repositories**: Data access using Dapper
4. **Infrastructure**: External service clients (Kubernetes, Stripe, Resend)
5. **Domain**: Entity models

### Key Components

- **TenantService**: Tenant lifecycle management and Kubernetes orchestration
- **BillingService**: Stripe subscription and payment handling
- **DemoRequestService**: Demo account workflow
- **DomainService**: Custom domain verification
- **AnalyticsService**: Platform metrics and reporting
- **SuperAdminService**: Admin user management

## Setup

### Prerequisites

- .NET 9 SDK
- PostgreSQL 16+
- Kubernetes cluster (for production)
- Stripe account
- Resend account (for emails)

### Configuration

1. Copy `appsettings.Secrets.json.example` to `appsettings.Secrets.json`
2. Fill in the required secrets:
   - Database connection string
   - JWT secret key
   - Stripe API keys
   - Resend API key

### Database Setup

Create the management database:

```sql
CREATE DATABASE mechanicbuddy_management;
```

Run migrations (see `../MechanicBuddy.Management.DbUp`).

### Running Locally

```bash
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5100
- HTTPS: https://localhost:7100
- Swagger UI: http://localhost:5100

## API Endpoints

### Authentication

- `POST /api/auth/login` - Login as super admin
- `POST /api/auth/change-password` - Change password

### Tenants

- `GET /api/tenants` - List all tenants
- `GET /api/tenants/{id}` - Get tenant by ID
- `POST /api/tenants` - Create new tenant
- `PUT /api/tenants/{id}` - Update tenant
- `POST /api/tenants/{tenantId}/suspend` - Suspend tenant
- `POST /api/tenants/{tenantId}/resume` - Resume tenant
- `DELETE /api/tenants/{tenantId}` - Delete tenant (super admin only)
- `GET /api/tenants/stats` - Get tenant statistics

### Demo Requests

- `POST /api/demorequests` - Submit demo request (public)
- `GET /api/demorequests` - List all demo requests
- `GET /api/demorequests/pending` - Get pending requests
- `POST /api/demorequests/{id}/approve` - Approve demo request
- `POST /api/demorequests/{id}/reject` - Reject demo request

### Billing

- `POST /api/billing/create-customer` - Create Stripe customer
- `POST /api/billing/create-subscription` - Create subscription
- `POST /api/billing/cancel-subscription` - Cancel subscription
- `GET /api/billing/history/{tenantId}` - Get billing history
- `GET /api/billing/revenue` - Get revenue analytics
- `POST /api/billing/webhook` - Stripe webhook endpoint (public)

### Domains

- `POST /api/domains/verify/initiate` - Start domain verification
- `POST /api/domains/verify/{domain}` - Verify domain
- `GET /api/domains/status/{domain}` - Get verification status
- `DELETE /api/domains/{tenantId}` - Remove custom domain

### Analytics

- `GET /api/analytics/overview` - Platform overview
- `GET /api/analytics/tenant/{tenantId}` - Tenant metrics
- `GET /api/analytics/revenue` - Revenue analytics
- `GET /api/analytics/top-tenants` - Top tenants by metric

### Super Admin

- `GET /api/superadmin` - List all admins (super admin only)
- `POST /api/superadmin` - Create admin (super admin only)
- `POST /api/superadmin/{id}/deactivate` - Deactivate admin
- `POST /api/superadmin/{id}/activate` - Activate admin
- `DELETE /api/superadmin/{id}` - Delete admin

## Authentication

The API uses JWT bearer tokens for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <token>
```

### Getting a Token

```bash
curl -X POST http://localhost:5100/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@mechanicbuddy.com",
    "password": "your-password"
  }'
```

## Authorization Policies

- **SuperAdminOnly**: Requires `super_admin` role
- **ActiveAdmin**: Requires active admin account

## Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Development, Staging, or Production
- `ConnectionStrings__Management`: Database connection string
- `Jwt__SecretKey`: JWT signing key
- `Stripe__SecretKey`: Stripe API key
- `Email__ResendApiKey`: Resend API key

## Kubernetes Integration

The Management API deploys tenant instances to Kubernetes. Each tenant gets:

- Dedicated namespace (`mb-{tenantId}`)
- API deployment with resource limits based on tier
- ClusterIP service
- PostgreSQL schema in shared database

## Monitoring

Health check endpoints:

- `/health` - Overall health
- `/health/ready` - Readiness check (includes DB)

## Development

### Adding a New Repository

1. Create interface in `Repositories/I{Name}Repository.cs`
2. Implement in `Repositories/{Name}Repository.cs`
3. Register in `Program.cs`

### Adding a New Service

1. Create service class in `Services/{Name}Service.cs`
2. Inject required repositories and infrastructure clients
3. Register in `Program.cs`

### Adding a New Controller

1. Create controller in `Controllers/{Name}Controller.cs`
2. Add `[Authorize]` attribute if authentication required
3. Use dependency injection for services

## License

Proprietary - MechanicBuddy
