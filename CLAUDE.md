# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MechanicBuddy is a self-hosted workshop management system for vehicle service centers. It handles work orders, client/vehicle profiles, inventory, invoicing, and PDF generation.

> This project is a fork of [CarCare](https://github.com/rene98c/carcareco) by rene98c.

## Development Commands

### Full Stack (Docker)
```bash
# Generate secrets (first time)
./scripts/setup-secrets.sh

# Start all services
docker compose up --build -d

# Access points:
# - UI: http://localhost:3025
# - API: http://localhost:15567/swagger
# - Mail preview: http://localhost:8025
```

### Frontend Only
```bash
cd frontend
npm install
npm run dev     # Development with Turbopack
npm run build   # Production build
npm run lint    # ESLint
```

### Backend Only
```bash
cd backend/src/MechanicBuddy.Http.Api
dotnet build
dotnet run

# Run database migrations
cd backend/src/DbUp
dotnet run
```

### Building the full backend solution
```bash
cd backend/src
dotnet build MechanicBuddy.sln
```

## Architecture

### Backend (.NET 9)

Layered architecture with NHibernate ORM:

- **MechanicBuddy.Http.Api** - ASP.NET Core Web API entry point. Controllers in `Controllers/` directory. Uses JWT authentication.
- **MechanicBuddy.Core.Application** - Business logic, services, authorization, PDF printing, rate limiting
- **MechanicBuddy.Core.Domain** - Domain entities (Work, Vehicle, Client, SparePart, etc.) and repository interfaces
- **MechanicBuddy.Core.Persistence.Postgres** - NHibernate mappings (`ClassMappings.cs`), PostgreSQL repositories
- **MechanicBuddy.Http.Api.Model** - Request/response DTOs
- **DbUp** - Database migrations (SQL scripts and C# code migrations in `scripts/`)

Key configuration:
- `appsettings.Secrets.json` contains DB connection, JWT secret, SMTP settings
- Multitenancy support - each company gets isolated schema

### Frontend (Next.js 15)

App Router structure with server components:

- **src/app/** - Route segments (home/, auth/, print/)
- **src/app/home/** - Main app routes: clients/, vehicles/, work/, inventory/, settings/, profile/
- **src/_components/** - Shared React components
- **src/_lib/server/** - Server-side utilities:
  - `session.ts` - JWT session management (encrypts API token in httpOnly cookie)
  - `query-api.ts` - HTTP client wrapper for backend API calls
  - `authorization-middleware.ts` - Auth checks

API communication: Frontend makes server-side calls to `API_URL`, uses `NEXT_PUBLIC_API_URL` for client-side resources.

### Database

PostgreSQL 16+ with multitenancy via schemas. Migrations run via DbUp project before API starts.

## Multi-Tenant Architecture

MechanicBuddy uses a **hybrid deployment model** with two distinct modes:

### Shared Deployment (Free/Demo Tiers)
- Namespace: `mechanicbuddy-free-tier`
- All free-tier tenants share API/Web services (2 replicas each)
- Each tenant gets a separate database on shared PostgreSQL cluster
- Provisioned via: Database creation + NPM routing (no Helm deployment)
- Max capacity: 100 tenants per instance
- DeploymentMode: `"shared"`

### Dedicated Deployment (Paid Tiers)
- Namespace: `tenant-{tenantId}` (per tenant)
- Each tenant gets dedicated PostgreSQL, API, and Web deployments
- Resource scaling based on tier (professional: 2 replicas, enterprise: 3 replicas)
- Provisioned via: Full Helm chart deployment
- DeploymentMode: `"dedicated"`

### Tenant Isolation
- **Database**: All tenants have separate databases (even on shared instances)
- **Routing**: JWT claims (`ClaimTypes.Spn`) route to correct database
- **Network**: Namespace isolation for paid tiers
- **Domains**: All tenants get `{tenantId}.mechanicbuddy.app` subdomain

### Key Components
- **TenantProvisioningService**: [MechanicBuddy.Management.Api/Services/TenantProvisioningService.cs](backend/src/MechanicBuddy.Management.Api/Services/TenantProvisioningService.cs)
  - `ProvisionFreeTierTenantAsync()`: Creates tenant on shared instance
  - `ProvisionDedicatedTenantAsync()`: Deploys dedicated Helm chart
- **Free-Tier Helm Chart**: [infrastructure/helm/charts/mechanicbuddy-free-tier/](infrastructure/helm/charts/mechanicbuddy-free-tier/)
- **Tenant Helm Chart**: [infrastructure/helm/charts/mechanicbuddy-tenant/](infrastructure/helm/charts/mechanicbuddy-tenant/)
- **Configuration**: [MechanicBuddy.Management.Api/Configuration/ProvisioningOptions.cs](backend/src/MechanicBuddy.Management.Api/Configuration/ProvisioningOptions.cs)

## Configuration

Backend secrets: `backend/src/MechanicBuddy.Http.Api/appsettings.Secrets.json`
Frontend env: `frontend/.env` (copy from `.env.example`)

Required env vars for frontend:
- `SERVER_SECRET` - must match backend's ConsumerSecret
- `SESSION_SECRET` - cookie encryption key
- `API_URL` / `NEXT_PUBLIC_API_URL` - backend endpoint

## CI/CD

GitHub Actions workflows in `.github/workflows/`:
- Skip backend build: include `[skip-backend]` in commit message
- Skip frontend build: include `[skip-frontend]` in commit message

## Default Login

Username: `admin`, Password: `carcare`
