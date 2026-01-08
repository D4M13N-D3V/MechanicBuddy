# Development Setup

This guide covers setting up a local development environment for MechanicBuddy.

## Prerequisites

| Requirement | Version | Purpose |
|-------------|---------|---------|
| Docker | 24+ | Container runtime |
| Docker Compose | 2.20+ | Multi-container orchestration |
| .NET SDK | 9.0 | Backend development |
| Node.js | 22+ | Frontend development |
| PostgreSQL | 16+ | Database (or use Docker) |
| Git | Latest | Version control |

## Quick Start with Docker

The fastest way to get a development environment running:

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/mechanicbuddy.git
cd mechanicbuddy
```

### 2. Generate Secrets

```bash
./scripts/setup-secrets.sh
```

This creates:
- `backend/src/MechanicBuddy.Http.Api/appsettings.Secrets.json`
- `frontend/.env`

### 3. Start All Services

```bash
docker compose up --build -d
```

### 4. Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| Web UI | http://localhost:3025 | Main application |
| API Swagger | http://localhost:15567/swagger | API documentation |
| Mail Preview | http://localhost:8025 | MailHog email testing |
| PostgreSQL | localhost:5432 | Direct database access |

**Default Credentials:**
- Username: `admin`
- Password: `carcare`

---

## Development with Hot Reload

For active development, use the override configuration which enables hot reload:

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml up
```

This mounts source directories as volumes and uses:
- `dotnet watch` for the backend
- `npm run dev` for the frontend

### Hot Reload Behavior

| Component | Change Type | Reload Method |
|-----------|-------------|---------------|
| Backend | C# files | Automatic recompile |
| Backend | appsettings | Manual restart |
| Frontend | TSX/TS | Fast Refresh |
| Frontend | CSS | Fast Refresh |
| Database | Migrations | Run `docker compose up migrate` |

---

## Manual Setup (Without Docker)

### Backend Setup

#### 1. Install .NET 9 SDK

```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version 9.0.100

# macOS
brew install dotnet@9

# Windows
winget install Microsoft.DotNet.SDK.9
```

#### 2. Setup PostgreSQL

```bash
# Create database
createdb mechanicbuddy

# Or using Docker
docker run -d \
  --name mechanicbuddy-db \
  -e POSTGRES_USER=carcare \
  -e POSTGRES_PASSWORD=carcare \
  -e POSTGRES_DB=mechanicbuddy \
  -p 5432:5432 \
  postgres:16
```

#### 3. Configure Backend

Create `backend/src/MechanicBuddy.Http.Api/appsettings.Secrets.json`:

```json
{
  "JwtOptions": {
    "Secret": "your-64-byte-hex-secret-key-here-generate-with-openssl-rand-hex-64",
    "ConsumerSecret": "your-32-byte-base64-key"
  },
  "DbOptions": {
    "Host": "localhost",
    "Port": 5432,
    "UserId": "carcare",
    "Password": "carcare",
    "Name": "mechanicbuddy",
    "MultiTenancy": {
      "Enabled": false
    }
  },
  "SmtpOptions": {
    "Host": "localhost",
    "Port": 1025,
    "User": "",
    "Password": ""
  },
  "Cors": {
    "Mode": "Development",
    "AppHost": "http://localhost:3000"
  }
}
```

#### 4. Run Migrations

```bash
cd backend/src/DbUp
dotnet run
```

#### 5. Start the API

```bash
cd backend/src/MechanicBuddy.Http.Api
dotnet run

# Or with hot reload
dotnet watch run
```

API will be available at `http://localhost:15567`

### Frontend Setup

#### 1. Install Node.js 22

```bash
# Using nvm (recommended)
nvm install 22
nvm use 22

# macOS
brew install node@22

# Using fnm
fnm install 22
fnm use 22
```

#### 2. Install Dependencies

```bash
cd frontend
npm install
```

#### 3. Configure Environment

Create `frontend/.env`:

```env
# Must match backend ConsumerSecret
SERVER_SECRET=your-32-byte-base64-key

# Generate with: openssl rand -base64 32
SESSION_SECRET=another-32-byte-base64-key

# Backend API URL (server-side)
API_URL=http://localhost:15567

# Backend API URL (client-side resources)
NEXT_PUBLIC_API_URL=http://localhost:15567

# Session configuration
NEXT_PUBLIC_SESSION_TIMEOUT=1500
NEXT_PUBLIC_SESSION_DIALOG_TIMEOUT=120
```

#### 4. Start Development Server

```bash
npm run dev
```

Frontend will be available at `http://localhost:3000`

---

## IDE Setup

### Visual Studio Code

Recommended extensions:

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "dbaeumer.vscode-eslint",
    "esbenp.prettier-vscode",
    "bradlc.vscode-tailwindcss",
    "prisma.prisma",
    "mtxr.sqltools",
    "mtxr.sqltools-driver-pg"
  ]
}
```

**Settings for workspace** (`.vscode/settings.json`):

```json
{
  "editor.formatOnSave": true,
  "editor.defaultFormatter": "esbenp.prettier-vscode",
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp"
  },
  "typescript.tsdk": "frontend/node_modules/typescript/lib"
}
```

### JetBrains Rider

- Open `backend/src/MechanicBuddy.sln` for backend
- Use integrated terminal for frontend

### Visual Studio

- Open `backend/src/MechanicBuddy.sln`
- Use launch profiles for debugging

---

## Database Management

### Connecting to PostgreSQL

```bash
# Via psql
psql -h localhost -U carcare -d mechanicbuddy

# Via Docker
docker compose exec db psql -U carcare -d mechanicbuddy
```

### Useful Commands

```sql
-- List all tables
\dt domain.*

-- Show table structure
\d domain.work

-- Check active connections
SELECT * FROM pg_stat_activity WHERE datname = 'mechanicbuddy';

-- Recent work orders
SELECT number, started_on, user_status FROM domain.work ORDER BY started_on DESC LIMIT 10;
```

### Running Migrations Manually

```bash
# Build and run DbUp
cd backend/src/DbUp
dotnet build
dotnet run

# Or via Docker
docker compose up migrate
```

### Creating New Migrations

1. Add SQL file to `backend/src/DbUp/scripts/`:

```
Script0007_YourMigrationName.sql
```

2. Or create a C# migration for complex operations:

```
Script0007_YourMigrationName.cs
```

Example C# migration:

```csharp
using DbUp.Engine;
using Npgsql;

namespace MechanicBuddy.DbUp.scripts
{
    public class Script0007_YourMigrationName : IScript
    {
        public string ProvideScript(Func<IDbCommand> dbCommandFactory)
        {
            // Return SQL or execute commands
            return @"
                ALTER TABLE domain.work ADD COLUMN new_field VARCHAR(100);
            ";
        }
    }
}
```

---

## Testing

### Backend Tests

```bash
cd backend/src
dotnet test
```

### Frontend Linting

```bash
cd frontend
npm run lint
```

### Type Checking

```bash
cd frontend
npm run build  # Includes type checking
```

---

## Common Issues

### Port Conflicts

If ports are already in use:

```bash
# Check what's using a port
lsof -i :15567
lsof -i :3000
lsof -i :5432

# Kill process
kill -9 <PID>
```

Or modify ports in `docker-compose.override.yml`:

```yaml
services:
  api:
    ports:
      - "15568:15567"  # Use different host port
```

### Database Connection Issues

1. **Verify PostgreSQL is running:**
   ```bash
   docker compose ps db
   ```

2. **Check connection string in appsettings:**
   ```json
   "DbOptions": {
     "Host": "db",  // Use "localhost" for non-Docker
     "Port": 5432
   }
   ```

3. **Reset database:**
   ```bash
   docker compose down -v  # Removes volumes
   docker compose up -d
   ```

### Frontend Build Errors

1. **Clear cache:**
   ```bash
   rm -rf frontend/.next
   rm -rf frontend/node_modules
   npm install
   ```

2. **Check Node version:**
   ```bash
   node --version  # Should be 22+
   ```

### Backend Build Errors

1. **Restore packages:**
   ```bash
   cd backend/src
   dotnet restore
   ```

2. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   ```

### Secrets Mismatch

If authentication fails:

1. Ensure `SERVER_SECRET` in `frontend/.env` matches `ConsumerSecret` in `appsettings.Secrets.json`

2. Regenerate secrets:
   ```bash
   ./scripts/setup-secrets.sh
   ```

---

## Development Workflow

### Typical Development Cycle

1. **Start services:**
   ```bash
   docker compose up -d
   ```

2. **Make changes** in your IDE

3. **Changes reload automatically** (hot reload)

4. **Test in browser** at http://localhost:3025

5. **Check API** at http://localhost:15567/swagger

6. **Review logs:**
   ```bash
   docker compose logs -f api
   docker compose logs -f web
   ```

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes and commit
git add .
git commit -m "feat: add new feature"

# Push and create PR
git push -u origin feature/my-feature
```

### CI Skip Flags

When pushing, you can skip certain builds:

- `[skip-backend]` - Skip backend build
- `[skip-frontend]` - Skip frontend build

Example:
```bash
git commit -m "docs: update readme [skip-backend] [skip-frontend]"
```

---

## Debugging

### Backend Debugging

**VS Code launch.json:**

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (API)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/backend/src/MechanicBuddy.Http.Api/bin/Debug/net9.0/MechanicBuddy.Http.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/backend/src/MechanicBuddy.Http.Api",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### Frontend Debugging

Use browser DevTools with React Developer Tools extension.

**VS Code launch.json for Chrome:**

```json
{
  "type": "chrome",
  "request": "launch",
  "name": "Launch Chrome",
  "url": "http://localhost:3000",
  "webRoot": "${workspaceFolder}/frontend"
}
```

### Database Debugging

Enable NHibernate SQL logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "NHibernate.SQL": "Debug"
    }
  }
}
```
