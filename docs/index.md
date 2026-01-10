# MechanicBuddy Documentation

Welcome to the MechanicBuddy documentation. MechanicBuddy is a self-hosted workshop management system for vehicle service centers, handling work orders, client/vehicle profiles, inventory, invoicing, and PDF generation.

!!! info "Fork Notice"
    This project is a fork of [CarCare](https://github.com/rene98c/carcareco) by rene98c.

## Documentation Sections

### Technical Documentation

For developers and system administrators who need to understand, deploy, or extend MechanicBuddy.

- **[Architecture Overview](technical/architecture.md)** - System design, component interactions, and code organization
- **[API Reference](technical/api-reference.md)** - Complete REST API documentation with endpoints and examples
- **[Database Schema](technical/database-schema.md)** - Table structures, relationships, and migration system

### User Guide

For workshop staff and administrators who use MechanicBuddy daily.

- **[Getting Started](user-guide/getting-started.md)** - First steps after installation
- **[Workshop Staff Guide](user-guide/workshop-staff-guide.md)** - Daily operations: work orders, clients, vehicles, invoicing
- **[Administrator Guide](user-guide/administrator-guide.md)** - System configuration, user management, branding

## Multi-Tenant Architecture

MechanicBuddy implements a hybrid multi-tenant architecture optimized for both cost efficiency and performance:

- **Free & Demo Tiers**: Tenants share a common deployment with isolated databases
- **Paid Tiers**: Tenants receive dedicated Kubernetes namespaces with full resource isolation

This approach allows us to:
- Support unlimited free-tier users cost-effectively
- Provide premium performance and isolation for paying customers
- Maintain strong security boundaries at the database level for all tiers

For detailed architecture information, see:
- [Technical Architecture](technical/architecture.md) - Complete system design and deployment patterns
- [Deployment Guide](technical/deployment.md) - Step-by-step deployment instructions
- [Management API](../backend/src/MechanicBuddy.Management.Api/README.md) - Tenant provisioning and management

## Quick Start

## Technology Stack

| Component | Technology |
|-----------|------------|
| Backend | .NET 9, ASP.NET Core, NHibernate |
| Frontend | Next.js 15, React 19, TypeScript, Tailwind CSS |
| Database | PostgreSQL 16+ |
| Containerization | Docker, Kubernetes |
| CI/CD | GitHub Actions, ArgoCD |

## Key Features

- **Work Order Management** - Create, track, and complete repair jobs
- **Client Management** - Private and business client profiles
- **Vehicle Registry** - Track vehicles with ownership history
- **Inventory Control** - Spare parts with storage locations
- **Invoicing** - Generate professional invoices and estimates
- **PDF Generation** - Automatic PDF creation for documents
- **Multi-tenancy** - SaaS deployment with isolated tenant databases
- **Customizable Branding** - Per-tenant logos, colors, and landing pages

## Support

- **Issues:** Report bugs and feature requests on [GitHub Issues](https://github.com/your-org/mechanicbuddy/issues)
- **Documentation:** You're looking at it!
