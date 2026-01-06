# MechanicBuddy Management API - Quick Start Guide

## Prerequisites

- .NET 9 SDK installed
- PostgreSQL 16+ running locally or accessible remotely
- (Optional) Docker for running PostgreSQL locally

## Step 1: Database Setup

### Option A: Using Docker

```bash
docker run --name mechanicbuddy-mgmt-db \
  -e POSTGRES_DB=mechanicbuddy_management \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:16
```

### Option B: Existing PostgreSQL

Create the database:

```sql
CREATE DATABASE mechanicbuddy_management;
```

## Step 2: Run Database Migrations

Navigate to the migrations project and run:

```bash
cd ../MechanicBuddy.Management.DbUp
dotnet run
```

This will create all necessary tables.

## Step 3: Configure Secrets

Copy the secrets template:

```bash
cd ../MechanicBuddy.Management.Api
cp appsettings.Secrets.json.example appsettings.Secrets.json
```

Edit `appsettings.Secrets.json` with your values:

```json
{
  "ConnectionStrings": {
    "Management": "Host=localhost;Port=5432;Database=mechanicbuddy_management;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters-long-replace-this-value"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Email": {
    "ResendApiKey": "re_..."
  }
}
```

### Generating a JWT Secret Key

```bash
# Linux/Mac
openssl rand -base64 32

# Or use this .NET command
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "$(openssl rand -base64 32)"
```

## Step 4: Create Initial Super Admin

### Option A: Using SQL Script

```bash
psql -h localhost -U postgres -d mechanicbuddy_management -f Scripts/CreateSuperAdmin.sql
```

### Option B: Using BCrypt Hash Generator

Run this C# snippet to generate a password hash:

```csharp
using BCrypt.Net;
var hash = BCrypt.HashPassword("YourPassword123!", 11);
Console.WriteLine(hash);
```

Then insert manually:

```sql
INSERT INTO super_admins (email, password_hash, name, role, is_active, created_at)
VALUES ('admin@mechanicbuddy.com', 'YOUR_BCRYPT_HASH', 'Admin User', 'super_admin', true, NOW());
```

## Step 5: Install Dependencies

```bash
dotnet restore
```

## Step 6: Run the API

```bash
dotnet run
```

The API will start on:
- HTTP: http://localhost:5100
- Swagger UI: http://localhost:5100/swagger

## Step 7: Test the API

### 1. Login

```bash
curl -X POST http://localhost:5100/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@mechanicbuddy.com",
    "password": "YourPassword123!"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "admin": {
    "id": 1,
    "email": "admin@mechanicbuddy.com",
    "name": "Admin User",
    "role": "super_admin"
  }
}
```

### 2. Use the Token

Save the token and use it in subsequent requests:

```bash
TOKEN="your-token-here"

curl -X GET http://localhost:5100/api/tenants \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Create a Demo Request (Public Endpoint)

```bash
curl -X POST http://localhost:5100/api/demorequests \
  -H "Content-Type: application/json" \
  -d '{
    "email": "demo@example.com",
    "companyName": "Test Company",
    "phoneNumber": "+1234567890"
  }'
```

### 4. Approve Demo Request

```bash
curl -X POST http://localhost:5100/api/demorequests/1/approve \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "notes": "Approved for testing"
  }'
```

## Step 8: Explore Swagger UI

Open http://localhost:5100 in your browser to access interactive API documentation.

1. Click "Authorize" button
2. Enter: `Bearer YOUR_TOKEN`
3. Click "Authorize"
4. Now you can test all endpoints interactively

## Common Issues

### Database Connection Failed

Check:
- PostgreSQL is running
- Connection string in `appsettings.Secrets.json` is correct
- Database exists and migrations ran successfully

```bash
psql -h localhost -U postgres -d mechanicbuddy_management -c "\dt"
```

### JWT Token Invalid

- Ensure `Jwt:SecretKey` in `appsettings.Secrets.json` is at least 32 characters
- Check that the token hasn't expired (default: 8 hours)
- Verify the token is included as: `Authorization: Bearer <token>`

### Stripe Errors (Optional)

If not using Stripe yet, you can skip billing operations. Set test keys:

```json
"Stripe": {
  "SecretKey": "sk_test_dummy",
  "WebhookSecret": "whsec_dummy"
}
```

### Email Errors (Optional)

If not using Resend yet, email sending will fail but won't break core functionality:

```json
"Email": {
  "ResendApiKey": "re_dummy"
}
```

## Development Workflow

1. **Make changes** to code
2. **Rebuild**: `dotnet build`
3. **Run**: `dotnet run`
4. **Test**: Use Swagger UI or curl

### Hot Reload (dotnet watch)

```bash
dotnet watch run
```

Changes to code will automatically rebuild and restart the API.

## Next Steps

1. **Configure Stripe** for billing integration
2. **Set up Kubernetes** for tenant provisioning (or use mock implementation)
3. **Configure Resend** for email notifications
4. **Create additional admins** via the SuperAdmin endpoints
5. **Set up monitoring** and logging
6. **Deploy to production** environment

## Production Considerations

Before deploying to production:

1. Change default admin password
2. Use strong JWT secret key
3. Enable HTTPS (set `UseHttpsRedirection` in Program.cs)
4. Configure CORS with specific origins
5. Set up proper logging (Application Insights, Seq, etc.)
6. Configure health checks for monitoring
7. Set up database backups
8. Use managed PostgreSQL (AWS RDS, Azure Database, etc.)
9. Store secrets in Azure Key Vault, AWS Secrets Manager, etc.
10. Set up CI/CD pipeline

## Support

For issues or questions:
- Check the logs in console output
- Review API responses for error details
- Check database connectivity
- Verify configuration in appsettings.json

## Useful Commands

```bash
# Check .NET version
dotnet --version

# Restore packages
dotnet restore

# Build project
dotnet build

# Run with watch (hot reload)
dotnet watch run

# Run tests (when added)
dotnet test

# Publish for deployment
dotnet publish -c Release
```

## API Health Checks

```bash
# Basic health check
curl http://localhost:5100/health

# Readiness check (includes DB)
curl http://localhost:5100/health/ready
```

Both should return `Healthy` status.
