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

MechanicBuddy supports two deployment architectures for multi-tenant SaaS:

1. **Free-Tier Shared Instance** - All free users share a single deployment with multi-tenancy
2. **Paid-Tier Dedicated Instances** - Each paid customer gets their own isolated namespace

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                      Management System                          │
│         (mechanicbuddy-system namespace)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐      │
│  │   Portal     │  │     API      │  │   PostgreSQL     │      │
│  │  (Next.js)   │  │   (.NET)     │  │  (Management DB) │      │
│  └──────────────┘  └──────────────┘  └──────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Provisions & Routes
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              Free-Tier Shared Instance                          │
│         (mechanicbuddy-free-tier namespace)                     │
│  ┌──────────────────────────────────────────────────────┐       │
│  │  Shared API (2 replicas)  │  Shared Web (2 replicas) │       │
│  └──────────────────────────────────────────────────────┘       │
│  Multi-tenancy ENABLED - Up to 100 tenants                      │
│  Uses shared PostgreSQL cluster from management system          │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│              Paid-Tier Dedicated Instances                      │
│                                                                 │
│  ┌────────────────────────┐  ┌────────────────────────┐        │
│  │ tenant-acme namespace  │  │ tenant-demo namespace  │        │
│  │ ┌──────┐ ┌──────┐      │  │ ┌──────┐ ┌──────┐     │        │
│  │ │ Web  │ │ API  │      │  │ │ Web  │ │ API  │     │        │
│  │ └──────┘ └──────┘      │  │ └──────┘ └──────┘     │        │
│  │ ┌──────────────────┐   │  │ ┌──────────────────┐  │        │
│  │ │   PostgreSQL     │   │  │ │   PostgreSQL     │  │        │
│  │ │  (Dedicated HA)  │   │  │ │  (Dedicated HA)  │  │        │
│  │ └──────────────────┘   │  │ └──────────────────┘  │        │
│  └────────────────────────┘  └────────────────────────┘        │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│              Routing Layer (NPM + Ingress)                      │
│  *.mechanicbuddy.app → Free-tier shared OR Dedicated instance  │
│  Custom domains → Dedicated instances only                      │
└─────────────────────────────────────────────────────────────────┘
```

### Deployment Type Comparison

| Feature | Free-Tier Shared | Paid-Tier Dedicated |
|---------|------------------|---------------------|
| Namespace | `mechanicbuddy-free-tier` | `tenant-{tenantId}` |
| Resources | Shared (2x API, 2x Web) | Dedicated per tenant |
| PostgreSQL | Shared cluster | Dedicated cluster |
| Multi-tenancy | Enabled (JWT-based) | Disabled (single tenant) |
| Max tenants | 100 per instance | 1 per namespace |
| Helm chart | `mechanicbuddy-free-tier` | `mechanicbuddy-tenant` |
| ArgoCD managed | Yes | No (API provisioned) |
| Scaling | Fixed | Based on tier |

---

## Free-Tier Shared Instance Deployment

### Overview

The free-tier uses a shared deployment where all free users access the same API and Web services. Multi-tenancy is enabled at the application layer, with tenant isolation handled via JWT claims and database schemas.

**Namespace:** `mechanicbuddy-free-tier`
**Helm Chart:** `infrastructure/helm/charts/mechanicbuddy-free-tier`
**ArgoCD App:** `infrastructure/argocd/apps/free-tier-production.yaml`

### Components

- **Shared API**: 2 replicas, multi-tenancy enabled, JWT-based routing
- **Shared Web**: 2 replicas, serves all free-tier tenants
- **External PostgreSQL**: Uses management system's PostgreSQL cluster
- **Resource Quota**: CPU: 4-8 cores, Memory: 8-16Gi, Pods: 20

### Prerequisites

1. Management system deployed (see below)
2. ArgoCD installed and configured
3. External PostgreSQL cluster available

### 1. Configure Values

Create `infrastructure/helm/charts/mechanicbuddy-free-tier/values-production.yaml`:

```yaml
# Free-tier production configuration
postgresql:
  host: "mechanicbuddy-management-db-rw.mechanicbuddy-system.svc.cluster.local"
  port: 5432
  database: "mechanicbuddy"
  username: "management"
  password: "your-secure-password"

api:
  replicas: 2
  image:
    repository: "ghcr.io/your-org/mechanicbuddy-api"
    tag: "v1.0.0"
  resources:
    requests:
      memory: "512Mi"
      cpu: "250m"
    limits:
      memory: "1Gi"
      cpu: "1000m"

web:
  replicas: 2
  image:
    repository: "ghcr.io/your-org/mechanicbuddy-web"
    tag: "v1.0.0"
  resources:
    requests:
      memory: "256Mi"
      cpu: "100m"
    limits:
      memory: "512Mi"
      cpu: "500m"

email:
  host: "smtp.resend.com"
  port: 587
  apiKey: "re_your_api_key"
  fromAddress: "noreply@mechanicbuddy.app"

secrets:
  jwtSecret: "your-64-byte-hex-jwt-secret"
  consumerSecret: "your-32-byte-base64-consumer-secret"
  sessionSecret: "your-32-byte-base64-session-secret"

resourceQuota:
  enabled: true
  requests:
    cpu: "4"
    memory: "8Gi"
  limits:
    cpu: "8"
    memory: "16Gi"
  pods: "20"
```

### 2. Deploy with ArgoCD

```bash
# Apply ArgoCD application
kubectl apply -f infrastructure/argocd/apps/free-tier-production.yaml

# Monitor deployment
argocd app get mechanicbuddy-free-tier
argocd app sync mechanicbuddy-free-tier

# Verify pods
kubectl -n mechanicbuddy-free-tier get pods
kubectl -n mechanicbuddy-free-tier get services
```

### 3. Verify Deployment

```bash
# Check API health
kubectl -n mechanicbuddy-free-tier exec -it deploy/free-tier-api -- \
  curl http://localhost:15567/health/live

# Check Web health
kubectl -n mechanicbuddy-free-tier exec -it deploy/free-tier-web -- \
  curl http://localhost:3000/api/health

# View logs
kubectl -n mechanicbuddy-free-tier logs -f deploy/free-tier-api
kubectl -n mechanicbuddy-free-tier logs -f deploy/free-tier-web
```

### Manual Deployment (without ArgoCD)

```bash
# Deploy using Helm directly
helm upgrade --install mechanicbuddy-free-tier \
  infrastructure/helm/charts/mechanicbuddy-free-tier \
  -f infrastructure/helm/charts/mechanicbuddy-free-tier/values.yaml \
  -f infrastructure/helm/charts/mechanicbuddy-free-tier/values-production.yaml \
  -n mechanicbuddy-free-tier --create-namespace
```

### Scaling Considerations

**Horizontal Scaling:**
```bash
# Scale API replicas
kubectl -n mechanicbuddy-free-tier scale deployment free-tier-api --replicas=4

# Scale Web replicas
kubectl -n mechanicbuddy-free-tier scale deployment free-tier-web --replicas=4
```

**Tenant Limits:**
- Maximum 100 tenants per free-tier instance (configurable)
- When limit reached, provision additional free-tier instance
- Load balanced via NPM routing layer

---

## Paid-Tier Dedicated Instance Deployment

### Overview

Each paid customer (Professional, Enterprise tiers) receives a dedicated namespace with isolated resources. The Management API automatically provisions these instances when a customer upgrades.

**Namespace Pattern:** `tenant-{tenantId}`
**Helm Chart:** `infrastructure/helm/charts/mechanicbuddy-tenant`
**Provisioning:** Automatic via Management API

### Components Per Tenant

- **Dedicated API**: 1-3 replicas (tier-based)
- **Dedicated Web**: 1-3 replicas (tier-based)
- **Dedicated PostgreSQL**: CloudNativePG cluster with HA
- **Ingress**: Tenant-specific ingress controller
- **Migration Jobs**: Automatic database schema initialization
- **Network Policies**: Tenant isolation

### Resource Allocation by Tier

| Tier | API Replicas | Web Replicas | PostgreSQL | Storage | Backup |
|------|-------------|--------------|------------|---------|--------|
| Professional | 2 | 2 | 1 instance | 20Gi | Daily |
| Enterprise | 3 | 3 | 3 instances (HA) | 50Gi | Hourly |

### Manual Provisioning (for testing)

#### 1. Create Values File

**tenant-acme-values.yaml:**

```yaml
tenant:
  id: "acme"
  name: "ACME Auto Repair"
  tier: "professional"
  ownerEmail: "owner@acmeauto.com"

domains:
  baseDomain: "mechanicbuddy.app"
  default: "acme.mechanicbuddy.app"
  custom:
    - "shop.acmeauto.com"
  clusterIssuer: "letsencrypt-prod"

postgresql:
  instances: 1
  database: "mechanicbuddy"
  username: "mechanicbuddy"
  storage:
    size: "20Gi"
    storageClass: "local-path"
  backup:
    enabled: true
    schedule: "0 2 * * *"
    retentionDays: 7

api:
  replicas: 2
  image:
    repository: "ghcr.io/your-org/mechanicbuddy-api"
    tag: "v1.0.0"

web:
  replicas: 2
  image:
    repository: "ghcr.io/your-org/mechanicbuddy-web"
    tag: "v1.0.0"

email:
  host: "smtp.resend.com"
  port: 587
  apiKey: "re_your_api_key"
  fromAddress: "noreply@mechanicbuddy.app"

billing:
  stripeCustomerId: "cus_xxx"
  subscriptionId: "sub_xxx"
  mechanicLimit: null  # Unlimited for paid tiers
```

#### 2. Deploy Tenant Instance

```bash
# Deploy using Helm
helm upgrade --install mechanicbuddy-tenant-acme \
  infrastructure/helm/charts/mechanicbuddy-tenant \
  -f tenant-acme-values.yaml \
  -n tenant-acme --create-namespace

# Wait for PostgreSQL cluster to be ready
kubectl -n tenant-acme wait --for=condition=Ready cluster/acme-postgresql --timeout=300s

# Verify deployment
kubectl -n tenant-acme get pods
kubectl -n tenant-acme get cluster
kubectl -n tenant-acme get ingress
```

#### 3. Verify Tenant Deployment

```bash
# Check PostgreSQL status
kubectl -n tenant-acme get cluster acme-postgresql

# Check API health
kubectl -n tenant-acme exec -it deploy/acme-api -- \
  curl http://localhost:15567/health/live

# View migration logs
kubectl -n tenant-acme logs job/acme-migration

# Test tenant access
curl https://acme.mechanicbuddy.app
```

### Automated Provisioning (Production)

The Management API handles tenant provisioning automatically:

1. Customer upgrades subscription via Stripe
2. Webhook triggers Management API
3. API creates namespace: `tenant-{tenantId}`
4. Deploys Helm chart with tier-specific values
5. PostgreSQL cluster created from template
6. Migrations run automatically
7. DNS record created (Cloudflare API)
8. NPM proxy rules configured
9. TLS certificate issued (cert-manager)
10. Tenant marked as active

### Tier-Specific Deployment

```bash
# Professional tier (from values file)
helm upgrade --install mechanicbuddy-tenant-acme \
  infrastructure/helm/charts/mechanicbuddy-tenant \
  -f infrastructure/helm/charts/mechanicbuddy-tenant/values/professional.yaml \
  --set tenant.id=acme \
  --set tenant.name="ACME Auto" \
  -n tenant-acme --create-namespace

# Enterprise tier (from values file)
helm upgrade --install mechanicbuddy-tenant-enterprise \
  infrastructure/helm/charts/mechanicbuddy-tenant \
  -f infrastructure/helm/charts/mechanicbuddy-tenant/values/enterprise.yaml \
  --set tenant.id=enterprise \
  --set tenant.name="Enterprise Corp" \
  -n tenant-enterprise --create-namespace
```

---

## Management System Deployment

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

### 1. Deploy Management System

Create `production-values.yaml`:

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
  cloudflareApiToken: "your-cloudflare-token"
  npmApiUrl: "http://nginx-proxy-manager:81/api"
  npmApiToken: "your-npm-token"
```

Install:

```bash
helm upgrade --install mechanicbuddy-system \
  infrastructure/helm/charts/mechanicbuddy-system \
  -f production-values.yaml \
  -n mechanicbuddy-system --create-namespace

# Verify
kubectl -n mechanicbuddy-system get pods
kubectl -n mechanicbuddy-system get cluster
```

### 2. Configure External Services

**Stripe Configuration:**
- Create webhook endpoint: `https://mechanicbuddy.app/api/webhooks/stripe`
- Subscribe to events: `customer.subscription.created`, `customer.subscription.updated`, `customer.subscription.deleted`
- Note webhook secret for configuration

**Cloudflare Configuration:**
- Create API token with DNS edit permissions
- Configure zone ID for `mechanicbuddy.app`

**NPM (Nginx Proxy Manager) Configuration:**
- Install NPM in cluster or external server
- Create API access token
- Configure base proxy rules

---

## Routing Configuration

### NPM (Nginx Proxy Manager) Setup

All tenant traffic routes through NPM, which determines whether to route to shared free-tier or dedicated instances.

#### Free-Tier Routing

```bash
# NPM Proxy Host configuration for *.mechanicbuddy.app
Source:
  Domain: *.mechanicbuddy.app
  Scheme: https

Forward:
  Hostname: free-tier-web.mechanicbuddy-free-tier.svc.cluster.local
  Port: 3000

Custom Config:
  # Extract tenant ID from subdomain
  set $tenant_id "";
  if ($host ~* ^([^.]+)\.mechanicbuddy\.app$) {
    set $tenant_id $1;
  }

  # Add tenant header for multi-tenant routing
  proxy_set_header X-Tenant-ID $tenant_id;
```

#### Paid-Tier Routing

```bash
# NPM creates dynamic proxy hosts per tenant
# Example: acme.mechanicbuddy.app
Source:
  Domain: acme.mechanicbuddy.app
  Scheme: https

Forward:
  Hostname: acme-web.tenant-acme.svc.cluster.local
  Port: 3000

TLS:
  Certificate: Auto-provision via cert-manager
```

#### Custom Domain Routing

```bash
# Example: shop.acmeauto.com → tenant-acme
Source:
  Domain: shop.acmeauto.com
  Scheme: https

Forward:
  Hostname: acme-web.tenant-acme.svc.cluster.local
  Port: 3000

TLS:
  Certificate: Custom (uploaded by tenant)
```

### DNS Configuration

**Subdomain Pattern:**
```
{tenantId}.mechanicbuddy.app → CNAME → mechanicbuddy.app
```

**Free-tier example:**
```
demo.mechanicbuddy.app → NPM → free-tier-web service
```

**Paid-tier example:**
```
acme.mechanicbuddy.app → NPM → acme-web service (tenant-acme namespace)
```

### Tenant Routing Flow

1. User navigates to `demo.mechanicbuddy.app`
2. DNS resolves to NPM load balancer
3. NPM extracts tenant ID: `demo`
4. Management API checks tenant tier: `free`
5. NPM routes to: `free-tier-web.mechanicbuddy-free-tier`
6. Web/API uses JWT to determine tenant database schema

For paid tier:

1. User navigates to `acme.mechanicbuddy.app`
2. DNS resolves to NPM load balancer
3. NPM extracts tenant ID: `acme`
4. Management API checks tenant tier: `professional`
5. NPM routes to: `acme-web.tenant-acme`
6. Dedicated instance serves request (no multi-tenancy needed)

---

## Infrastructure Components Summary

### Namespaces

| Namespace | Purpose | Components |
|-----------|---------|------------|
| `mechanicbuddy-system` | Management system | API, Portal, PostgreSQL, Management services |
| `mechanicbuddy-free-tier` | Free-tier shared | API, Web (multi-tenant), uses system PostgreSQL |
| `tenant-{tenantId}` | Paid tenant | API, Web, PostgreSQL (dedicated) |

### Helm Charts

| Chart | Purpose | Deployed By |
|-------|---------|-------------|
| `mechanicbuddy-system` | Management infrastructure | Manual/ArgoCD |
| `mechanicbuddy-free-tier` | Shared free-tier instance | ArgoCD |
| `mechanicbuddy-tenant` | Individual paid tenants | Management API |

### Kubectl Commands

```bash
# List all MechanicBuddy namespaces
kubectl get namespaces | grep mechanic

# View system status
kubectl -n mechanicbuddy-system get all

# View free-tier status
kubectl -n mechanicbuddy-free-tier get all

# View tenant status
kubectl -n tenant-acme get all

# List all tenant namespaces
kubectl get namespaces -l app.kubernetes.io/managed-by=mechanicbuddy-management

# Get tenant PostgreSQL clusters
kubectl get clusters -A | grep tenant

# Check resource usage across all tenants
kubectl top pods -A | grep tenant
```

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
