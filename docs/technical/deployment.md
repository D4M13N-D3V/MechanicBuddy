# Deployment Guide

MechanicBuddy supports multiple deployment options from simple Docker Compose to production Kubernetes clusters.

## Deployment Options Overview

| Option | Best For | Complexity |
|--------|----------|------------|
| Docker Compose | Small workshops, single server | Low |
| Traditional | Existing servers without containers | Medium |
| Kubernetes | Multi-tenant SaaS, high availability | High |

---

## Docker Compose Deployment

### Single Server Setup

Ideal for small workshops running on a single server.

#### 1. Prerequisites

- Ubuntu 22.04+ or similar Linux
- Docker 24+ and Docker Compose 2.20+
- Domain name pointed to server IP
- SSL certificate (Let's Encrypt recommended)

#### 2. Clone and Configure

```bash
# Clone repository
git clone https://github.com/your-org/mechanicbuddy.git
cd mechanicbuddy

# Generate secrets
./scripts/setup-secrets.sh

# Edit configuration
nano backend/src/MechanicBuddy.Http.Api/appsettings.Secrets.json
nano frontend/.env
```

#### 3. Production Configuration

**appsettings.Secrets.json:**

```json
{
  "JwtOptions": {
    "Secret": "your-production-64-byte-hex-secret",
    "ConsumerSecret": "your-production-32-byte-base64"
  },
  "DbOptions": {
    "Host": "db",
    "Port": 5432,
    "UserId": "mechanicbuddy",
    "Password": "strong-production-password",
    "Name": "mechanicbuddy",
    "MultiTenancy": {
      "Enabled": false
    }
  },
  "SmtpOptions": {
    "Host": "smtp.your-provider.com",
    "Port": 587,
    "User": "your-email@domain.com",
    "Password": "smtp-password"
  },
  "Cors": {
    "Mode": "Production",
    "AppHost": "https://your-domain.com"
  }
}
```

**frontend/.env:**

```env
SERVER_SECRET=your-production-32-byte-base64
SESSION_SECRET=another-32-byte-base64-key
API_URL=http://api:15567
NEXT_PUBLIC_API_URL=https://api.your-domain.com
NEXT_PUBLIC_SESSION_TIMEOUT=1500
```

#### 4. Start Services

```bash
docker compose up -d
```

#### 5. Setup Reverse Proxy

**Nginx configuration:**

```nginx
server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:3025;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_cache_bypass $http_upgrade;
    }
}

server {
    listen 443 ssl http2;
    server_name api.your-domain.com;

    ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:15567;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---

## Traditional Deployment

### Server Requirements

- Ubuntu 22.04+ or similar Linux
- .NET 9.0 Runtime
- Node.js 22+
- PostgreSQL 16+
- Nginx or similar reverse proxy
- PM2 (for Node.js process management)

### Backend Deployment

#### 1. Install .NET Runtime

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --runtime aspnetcore --version 9.0.0
```

#### 2. Build and Deploy

```bash
# Build release
cd backend/src/MechanicBuddy.Http.Api
dotnet publish -c Release -o /opt/mechanicbuddy/api

# Copy secrets
cp appsettings.Secrets.json /opt/mechanicbuddy/api/
```

#### 3. Create Systemd Service

**/etc/systemd/system/mechanicbuddy-api.service:**

```ini
[Unit]
Description=MechanicBuddy API
After=network.target postgresql.service

[Service]
WorkingDirectory=/opt/mechanicbuddy/api
ExecStart=/usr/bin/dotnet MechanicBuddy.Http.Api.dll
Restart=always
RestartSec=10
User=mechanicbuddy
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:15567

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable mechanicbuddy-api
sudo systemctl start mechanicbuddy-api
```

### Frontend Deployment

#### 1. Build Frontend

```bash
cd frontend
npm ci
npm run build
rm -rf src  # Remove source for security
```

#### 2. Deploy with PM2

```bash
# Install PM2
npm install -g pm2

# Start frontend
cd /opt/mechanicbuddy/frontend
pm2 start npm --name "mechanicbuddy-web" -- start

# Save PM2 config
pm2 save
pm2 startup
```

---

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster 1.28+
- kubectl configured
- Helm 3.12+
- cert-manager for TLS
- NGINX Ingress Controller
- CloudNativePG operator (for PostgreSQL)

### Install Prerequisites

```bash
# cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# NGINX Ingress
helm upgrade --install ingress-nginx ingress-nginx \
  --repo https://kubernetes.github.io/ingress-nginx \
  --namespace ingress-nginx --create-namespace

# CloudNativePG
kubectl apply -f https://raw.githubusercontent.com/cloudnative-pg/cloudnative-pg/release-1.22/releases/cnpg-1.22.0.yaml
```

### Deploy with Kustomize

#### 1. Configure Secrets

Create `infrastructure/k8s/overlays/production/secrets.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: mechanicbuddy-secrets
  namespace: mechanicbuddy-system
type: Opaque
stringData:
  db-password: "your-production-db-password"
  jwt-secret: "your-64-byte-hex-jwt-secret"
  consumer-secret: "your-32-byte-base64-consumer-secret"
  session-secret: "your-32-byte-base64-session-secret"
  smtp-password: "your-smtp-password"
```

#### 2. Update Kustomization

Edit `infrastructure/k8s/overlays/production/kustomization.yaml`:

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization

namespace: mechanicbuddy-system

resources:
  - ../../base
  - secrets.yaml

images:
  - name: ghcr.io/your-org/mechanicbuddy-api
    newTag: v1.0.0
  - name: ghcr.io/your-org/mechanicbuddy-web
    newTag: v1.0.0
  - name: ghcr.io/your-org/mechanicbuddy-dbup
    newTag: v1.0.0

patches:
  - path: patches/ingress.yaml
  - path: patches/replicas.yaml
```

#### 3. Deploy

```bash
# Apply configuration
kubectl apply -k infrastructure/k8s/overlays/production

# Verify deployment
kubectl -n mechanicbuddy-system get pods
kubectl -n mechanicbuddy-system get ingress
```

### Deploy with Helm

#### 1. Create Values File

**production-values.yaml:**

```yaml
# System deployment values
postgresql:
  enabled: true
  size: 20Gi
  instances: 2

managementApi:
  replicas: 2
  image:
    repository: ghcr.io/your-org/mechanicbuddy-management-api
    tag: v1.0.0

managementPortal:
  replicas: 2
  image:
    repository: ghcr.io/your-org/mechanicbuddy-management-portal
    tag: v1.0.0

ingress:
  enabled: true
  host: mechanicbuddy.app
  tls:
    enabled: true
    secretName: mechanicbuddy-tls

secrets:
  dbPassword: "your-db-password"
  jwtSecret: "your-jwt-secret"
  stripeSecretKey: "sk_live_xxx"
  stripeWebhookSecret: "whsec_xxx"
```

#### 2. Install Chart

```bash
helm upgrade --install mechanicbuddy-system \
  infrastructure/helm/charts/mechanicbuddy-system \
  -f production-values.yaml \
  -n mechanicbuddy-system --create-namespace
```

---

## Multi-Tenant SaaS Deployment

For running MechanicBuddy as a SaaS platform with multiple tenants.

### Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Management System                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │  Portal     │  │  API        │  │  PostgreSQL     │  │
│  │  (Next.js)  │  │  (.NET)     │  │  (Management)   │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────┘
                           │
                           │ Creates
                           ▼
┌─────────────────────────────────────────────────────────┐
│                    Tenant Instances                      │
│  ┌─────────────────────┐  ┌─────────────────────────┐   │
│  │ Tenant: ACME        │  │ Tenant: Demo            │   │
│  │ ┌─────┐ ┌─────┐     │  │ ┌─────┐ ┌─────┐       │   │
│  │ │ Web │ │ API │     │  │ │ Web │ │ API │       │   │
│  │ └─────┘ └─────┘     │  │ └─────┘ └─────┘       │   │
│  │ ┌─────────────┐     │  │ ┌─────────────┐       │   │
│  │ │ PostgreSQL  │     │  │ │ PostgreSQL  │       │   │
│  │ └─────────────┘     │  │ └─────────────┘       │   │
│  └─────────────────────┘  └─────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

### 1. Deploy Management System

```bash
# Install management system
helm upgrade --install mechanicbuddy-system \
  infrastructure/helm/charts/mechanicbuddy-system \
  -f production-values.yaml \
  -n mechanicbuddy-system --create-namespace
```

### 2. Configure External Services

The management API needs access to:

- **Stripe** - Payment processing
- **Cloudflare** - DNS automation
- **NPM** - Nginx Proxy Manager for routing

### 3. Tenant Provisioning

When a new customer signs up:

1. Management API creates Kubernetes namespace
2. Deploys tenant Helm chart
3. Creates PostgreSQL database from template
4. Runs migrations
5. Configures DNS record (Cloudflare)
6. Sets up TLS certificate

---

## GitOps with ArgoCD

### Install ArgoCD

```bash
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

### Configure Application

**infrastructure/argocd/apps/system-production.yaml:**

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: mechanicbuddy-system-production
  namespace: argocd
spec:
  project: mechanicbuddy
  source:
    repoURL: https://github.com/your-org/mechanicbuddy
    targetRevision: main
    path: infrastructure/k8s/overlays/production
  destination:
    server: https://kubernetes.default.svc
    namespace: mechanicbuddy-system
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

### Automated Deployments

The GitOps workflow automatically:

1. Builds Docker images on push to main
2. Updates image tags in Kustomization
3. Commits changes back to repo
4. ArgoCD detects changes and syncs

---

## Monitoring & Maintenance

### Health Checks

```bash
# API health
curl https://api.your-domain.com/health/live
curl https://api.your-domain.com/health/ready

# Web health
curl https://your-domain.com/api/health
```

### Logs

```bash
# Docker Compose
docker compose logs -f api
docker compose logs -f web

# Kubernetes
kubectl -n mechanicbuddy-system logs -f deployment/management-api
kubectl -n mechanicbuddy-system logs -f deployment/management-portal
```

### Database Backups

```bash
# Manual backup
pg_dump -h localhost -U mechanicbuddy -d mechanicbuddy > backup.sql

# Kubernetes (CloudNativePG)
kubectl -n mechanicbuddy-system exec -it mechanicbuddy-management-db-1 -- \
  pg_dump -U mechanicbuddy > backup.sql
```

### Updates

```bash
# Docker Compose
docker compose pull
docker compose up -d

# Kubernetes
kubectl set image deployment/management-api \
  api=ghcr.io/your-org/mechanicbuddy-api:v1.1.0 \
  -n mechanicbuddy-system
```

---

## Security Checklist

- [ ] Strong, unique secrets generated
- [ ] TLS/HTTPS enabled everywhere
- [ ] Database credentials not in version control
- [ ] CORS configured for production domain only
- [ ] Rate limiting enabled
- [ ] Regular backups configured
- [ ] Security updates automated
- [ ] Network policies in place (Kubernetes)
- [ ] Non-root containers
- [ ] Resource limits set

---

## Troubleshooting

### API Not Starting

```bash
# Check logs
docker compose logs api
kubectl -n mechanicbuddy-system logs deployment/management-api

# Common issues:
# - Database connection failed
# - Missing secrets configuration
# - Port already in use
```

### Database Connection Issues

```bash
# Test connection
psql -h localhost -U mechanicbuddy -d mechanicbuddy

# Check service
docker compose ps db
kubectl -n mechanicbuddy-system get pods -l app=postgresql
```

### Ingress Not Working

```bash
# Check ingress
kubectl -n mechanicbuddy-system get ingress
kubectl -n mechanicbuddy-system describe ingress

# Check cert-manager
kubectl get certificates -A
kubectl describe certificate mechanicbuddy-tls -n mechanicbuddy-system
```

### Slow Performance

1. Check resource limits
2. Review database indexes
3. Enable connection pooling
4. Scale replicas if needed
