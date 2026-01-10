# MechanicBuddy Helm Charts

Helm charts for deploying MechanicBuddy SaaS platform on Kubernetes.

## Charts Overview

| Chart | Purpose | Namespace | Deployment Method |
|-------|---------|-----------|-------------------|
| `mechanicbuddy-system` | Management infrastructure (portal, API, billing, provisioning) | `mechanicbuddy-system` | Manual (one-time) |
| `mechanicbuddy-free-tier` | Shared instance for free/demo tier tenants | `mechanicbuddy-free-tier` | ArgoCD (continuous) |
| `mechanicbuddy-tenant` | Dedicated per-tenant deployment for paid tiers | `tenant-{tenantId}` | Automated (via Management API) |

### Chart Details

#### mechanicbuddy-system

**Purpose**: Core management infrastructure for the SaaS platform.

**Components**:
- Management Portal (Next.js) - Admin dashboard for platform management
- Management API (.NET 9) - Tenant provisioning, billing, super-admin operations
- Shared PostgreSQL - Stores tenant metadata, billing info, user accounts
- Management DB migrations - Schema management for the management database

**Location**: `/infrastructure/helm/charts/mechanicbuddy-system/`

**Deployment**: Deployed once manually during initial platform setup.

#### mechanicbuddy-free-tier

**Purpose**: Provides shared infrastructure for free and demo tier tenants to optimize resource utilization.

**Components**:
- Shared API (2 replicas) - Multi-tenant aware API instances
- Shared Web (2 replicas) - Multi-tenant aware frontend instances
- Ingress with tenant routing - Routes requests based on hostname/subdomain
- Auto-scaling - HPA configured for both API and Web

**Database**: Uses external PostgreSQL from the management cluster with per-tenant schema isolation.

**Location**: `/infrastructure/helm/charts/mechanicbuddy-free-tier/`

**Capacity**: Configured to support up to 100 tenants (adjustable via values).

**Deployment**: Deployed once via ArgoCD for continuous deployment and automatic updates.

**Scaling Strategy**:
- Horizontal: 2-10 replicas based on CPU/memory usage
- Resource limits: Shared resources with fair-use policies
- Tenant isolation: Schema-level isolation in shared database

#### mechanicbuddy-tenant

**Purpose**: Dedicated deployment for paid tier tenants (Professional and Enterprise).

**Components**:
- PostgreSQL Cluster (CloudNativePG) - Isolated database per tenant
- API Deployment (.NET 9) - Dedicated API instances
- Web Deployment (Next.js) - Dedicated frontend instances
- Migration Job - Database schema initialization
- Ingress - Custom domain and subdomain routing
- Secrets - Auto-generated JWT keys, database credentials

**Location**: `/infrastructure/helm/charts/mechanicbuddy-tenant/`

**Namespace**: Dynamically created as `tenant-{tenantId}` for each customer.

**Deployment**: Automatically deployed by Management API via TenantProvisioningService when a tenant upgrades to paid tier.

**Resource Allocation**:
- **Professional**: 2 CPU, 4GB RAM, 50GB storage
- **Enterprise**: 4 CPU, 8GB RAM, 200GB storage

## Prerequisites

1. **Kubernetes cluster** with:
   - ingress-nginx controller
   - cert-manager with ClusterIssuer
   - CloudNativePG operator
   - local-path-provisioner (or other storage class)

2. **External services**:
   - Stripe account (for billing)
   - Resend account (for email)
   - Domain with DNS configured

## Deployment Workflow

The MechanicBuddy platform uses a staged deployment approach:

1. **System Chart** (Manual, One-Time)
   - Deployed during initial platform setup
   - Provides management infrastructure for the entire platform
   - Required before deploying any tenant instances

2. **Free-Tier Chart** (ArgoCD, One-Time)
   - Deployed once after system chart is ready
   - Continuously updated via ArgoCD
   - Automatically handles free and demo tenant routing

3. **Tenant Chart** (Automated, Per-Tenant)
   - Deployed automatically when a tenant upgrades to paid tier
   - Managed by Management API's TenantProvisioningService
   - Each deployment is isolated in its own namespace

## Quick Start

### 1. Deploy Management System

```bash
# Create namespace
kubectl create namespace mechanicbuddy-system

# Create secrets file (don't commit!)
cat > values-secrets.yaml << EOF
stripe:
  publishableKey: "pk_live_xxx"
  secretKey: "sk_live_xxx"
  webhookSecret: "whsec_xxx"

email:
  apiKey: "re_xxx"

superAdmin:
  initialEmail: "admin@mechanicbuddy.app"
  initialPassword: "secure-password"

database:
  password: "secure-db-password"
EOF

# Install
helm install mechanicbuddy-system ./charts/mechanicbuddy-system \
  --namespace mechanicbuddy-system \
  -f values-secrets.yaml \
  --set global.domain=mechanicbuddy.app

# Wait for deployment to be ready
kubectl wait --for=condition=available --timeout=300s \
  deployment/management-api -n mechanicbuddy-system
```

### 2. Deploy Free-Tier Infrastructure

```bash
# Create namespace
kubectl create namespace mechanicbuddy-free-tier

# Create ArgoCD application (if using ArgoCD)
cat <<EOF | kubectl apply -f -
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: mechanicbuddy-free-tier
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/your-org/mechanicbuddy
    targetRevision: main
    path: infrastructure/helm/charts/mechanicbuddy-free-tier
    helm:
      valueFiles:
        - values.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: mechanicbuddy-free-tier
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
EOF

# Or install manually with Helm
helm install mechanicbuddy-free-tier ./charts/mechanicbuddy-free-tier \
  --namespace mechanicbuddy-free-tier \
  --set database.host=mechanicbuddy-system-postgresql.mechanicbuddy-system.svc.cluster.local \
  --set database.name=mechanicbuddy \
  --set maxTenants=100

# Wait for deployment
kubectl wait --for=condition=available --timeout=300s \
  deployment/free-tier-api -n mechanicbuddy-free-tier
```

### 3. Deploy a Tenant (Manual Testing)

**Note**: In production, tenant deployments are automated by the Management API. Manual deployment is for testing only.

#### Demo Tenant (Free Tier)
Free tier tenants are automatically routed to the shared free-tier infrastructure. No separate deployment needed.

#### Professional Tenant

```bash
# This is typically done by Management API, but can be done manually for testing
helm install acme-workshop ./charts/mechanicbuddy-tenant \
  --create-namespace \
  --namespace tenant-acme \
  --set tenant.id=acme \
  --set tenant.name="ACME Auto Repair" \
  --set tenant.ownerEmail="admin@acme.com" \
  --set tenant.tier=professional \
  --set billing.stripeCustomerId=cus_xxx \
  -f ./charts/mechanicbuddy-tenant/values/professional.yaml
```

#### Enterprise Tenant

```bash
helm install bigshop ./charts/mechanicbuddy-tenant \
  --create-namespace \
  --namespace tenant-bigshop \
  --set tenant.id=bigshop \
  --set tenant.name="BigShop Motors" \
  --set tenant.ownerEmail="admin@bigshop.com" \
  --set tenant.tier=enterprise \
  --set billing.stripeCustomerId=cus_yyy \
  -f ./charts/mechanicbuddy-tenant/values/enterprise.yaml
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            KUBERNETES CLUSTER                               │
├─────────────────────────────────────────────────────────────────────────────┤
│  mechanicbuddy-system namespace                                             │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │ Management Portal│  │ Management API   │  │ Management DB    │          │
│  │ (Next.js)        │  │ (.NET 9)         │  │ (PostgreSQL)     │          │
│  │                  │  │ - Provisioning   │  │ - Tenant metadata│          │
│  │                  │  │ - Billing        │  │ - User accounts  │          │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘          │
│                                                                             │
│  Shared Services: ingress-nginx, cert-manager, CloudNativePG, ArgoCD       │
├─────────────────────────────────────────────────────────────────────────────┤
│  mechanicbuddy-free-tier namespace (shared free/demo tenants)              │
│  ┌──────────────────┐  ┌──────────────────┐                                │
│  │ Shared API       │  │ Shared Web       │                                │
│  │ (2-10 replicas)  │  │ (2-10 replicas)  │                                │
│  │ Multi-tenant     │  │ Multi-tenant     │                                │
│  └──────────────────┘  └──────────────────┘                                │
│                                                                             │
│  Database: Uses Management DB with per-tenant schemas                      │
│  Capacity: Up to 100 tenants                                               │
│  Routing: Hostname-based tenant resolution (X-Tenant-ID header)            │
│  Managed by: ArgoCD (continuous deployment)                                │
├─────────────────────────────────────────────────────────────────────────────┤
│  tenant-{id} namespace (per paid customer - Professional/Enterprise)       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                      │
│  │ Frontend     │  │ API          │  │ PostgreSQL   │                      │
│  │ (Next.js)    │  │ (.NET 9)     │  │ Cluster      │                      │
│  │ Dedicated    │  │ Dedicated    │  │ (isolated)   │                      │
│  └──────────────┘  └──────────────┘  └──────────────┘                      │
│                                                                             │
│  Custom domain: workshop.customerdomain.com                                 │
│  Default subdomain: {tenant-id}.mechanicbuddy.app                           │
│  Deployed by: Management API (TenantProvisioningService)                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Tenant Routing Decision

```
New Tenant Signup
       │
       ├─→ Free Tier?
       │      └─→ Yes → Route to mechanicbuddy-free-tier (shared)
       │                No deployment needed
       │
       └─→ Paid Tier (Professional/Enterprise)?
              └─→ Yes → Deploy mechanicbuddy-tenant chart
                        Create namespace: tenant-{tenantId}
                        Provision dedicated resources
```

## DNS Configuration

### Management Portal
```
mechanicbuddy.app      -> Ingress IP
api.mechanicbuddy.app  -> Ingress IP
```

### Tenant Subdomains
```
*.mechanicbuddy.app    -> Ingress IP (wildcard)
```

### Custom Domains
Each custom domain requires:
1. DNS TXT record for verification
2. CNAME/A record pointing to ingress

## Pricing Tiers

| Tier | Mechanics | Price | Infrastructure |
|------|-----------|-------|----------------|
| Free | 1 | $0/month | Shared (free-tier) |
| Professional | 2-10 | $20/mechanic/month | Dedicated |
| Enterprise | 11+ | $15/mechanic/month | Dedicated |

Demo: 7-day free trial with sample data (uses free-tier infrastructure).

## Configuration

### System Chart Configuration

Key values in `mechanicbuddy-system/values.yaml`:

```yaml
global:
  domain: mechanicbuddy.app
  ingressClassName: nginx
  certIssuer: letsencrypt-prod

stripe:
  publishableKey: "pk_live_xxx"
  secretKey: "sk_live_xxx"
  webhookSecret: "whsec_xxx"

email:
  apiKey: "re_xxx"
  fromEmail: "noreply@mechanicbuddy.app"

superAdmin:
  initialEmail: "admin@mechanicbuddy.app"
  initialPassword: "change-me"

database:
  storageClass: local-path
  storage: 100Gi
  password: "secure-password"
```

### Free-Tier Chart Configuration

Key values in `mechanicbuddy-free-tier/values.yaml`:

```yaml
maxTenants: 100

api:
  replicaCount: 2
  resources:
    requests:
      cpu: 1000m
      memory: 2Gi
    limits:
      cpu: 2000m
      memory: 4Gi
  autoscaling:
    enabled: true
    minReplicas: 2
    maxReplicas: 10

web:
  replicaCount: 2
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi
  autoscaling:
    enabled: true
    minReplicas: 2
    maxReplicas: 10

database:
  host: mechanicbuddy-system-postgresql.mechanicbuddy-system.svc.cluster.local
  name: mechanicbuddy
  port: 5432
```

### Tenant Chart Configuration

The tenant chart has tier-specific value files:

**Professional Tier** (`mechanicbuddy-tenant/values/professional.yaml`):
```yaml
api:
  replicaCount: 2
  resources:
    requests:
      cpu: 1000m
      memory: 2Gi
    limits:
      cpu: 2000m
      memory: 4Gi

web:
  replicaCount: 2
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi

database:
  instances: 2
  storage: 50Gi
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi
```

**Enterprise Tier** (`mechanicbuddy-tenant/values/enterprise.yaml`):
```yaml
api:
  replicaCount: 3
  resources:
    requests:
      cpu: 2000m
      memory: 4Gi
    limits:
      cpu: 4000m
      memory: 8Gi

web:
  replicaCount: 3
  resources:
    requests:
      cpu: 1000m
      memory: 2Gi
    limits:
      cpu: 2000m
      memory: 4Gi

database:
  instances: 3
  storage: 200Gi
  resources:
    requests:
      cpu: 1000m
      memory: 2Gi
    limits:
      cpu: 2000m
      memory: 4Gi
```

## Maintenance

### System Operations

#### Check system health
```bash
kubectl get pods -n mechanicbuddy-system
kubectl get cluster -n mechanicbuddy-system  # Management PostgreSQL
kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=management-api
```

#### View management portal
```bash
kubectl port-forward -n mechanicbuddy-system svc/management-portal 3000:80
# Access at http://localhost:3000
```

### Free-Tier Operations

#### Check free-tier health
```bash
kubectl get pods -n mechanicbuddy-free-tier
kubectl get hpa -n mechanicbuddy-free-tier  # Check auto-scaling
kubectl top pods -n mechanicbuddy-free-tier  # Resource usage
```

#### View free-tier logs
```bash
# API logs
kubectl logs -n mechanicbuddy-free-tier -l app.kubernetes.io/component=api --tail=100

# Web logs
kubectl logs -n mechanicbuddy-free-tier -l app.kubernetes.io/component=web --tail=100

# Follow logs in real-time
kubectl logs -n mechanicbuddy-free-tier -l app.kubernetes.io/component=api -f
```

#### Check free-tier capacity
```bash
# Count active free-tier tenants
kubectl exec -n mechanicbuddy-system deployment/management-api -- \
  psql -U mechanicbuddy -c "SELECT COUNT(*) FROM tenants WHERE tier = 'free';"

# View resource metrics
kubectl top pods -n mechanicbuddy-free-tier
```

#### Scale free-tier manually (if needed)
```bash
# Scale API
kubectl scale deployment free-tier-api -n mechanicbuddy-free-tier --replicas=5

# Scale Web
kubectl scale deployment free-tier-web -n mechanicbuddy-free-tier --replicas=5
```

### Tenant Operations (Paid Tiers)

#### List all paid tenants
```bash
kubectl get namespaces -l mechanicbuddy.app/namespace-type=tenant
```

#### Check specific tenant status
```bash
kubectl get pods -n tenant-{id}
kubectl get cluster -n tenant-{id}  # PostgreSQL
kubectl get ingress -n tenant-{id}
kubectl get certificate -n tenant-{id}
```

#### View tenant logs
```bash
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=api --tail=100
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=web --tail=100
```

#### Check tenant database
```bash
# Database status
kubectl get cluster -n tenant-{id}

# Database connection info
kubectl get secret -n tenant-{id} tenant-{id}-postgresql-app -o jsonpath='{.data.password}' | base64 -d

# Database backups
kubectl get backup -n tenant-{id}
```

#### Tenant resource usage
```bash
kubectl top pods -n tenant-{id}
kubectl describe quota -n tenant-{id}
```

#### Delete a tenant (caution!)
```bash
# This will permanently delete all tenant data
helm uninstall tenant-{id}
kubectl delete namespace tenant-{id}

# Or use the Management API (recommended)
curl -X DELETE https://api.mechanicbuddy.app/api/tenants/{id} \
  -H "Authorization: Bearer ${ADMIN_TOKEN}"
```

### Monitoring All Deployments

#### Check all MechanicBuddy resources
```bash
# All namespaces
kubectl get namespaces -l mechanicbuddy.app/managed-by=helm

# All pods across platform
kubectl get pods -A -l app.kubernetes.io/part-of=mechanicbuddy

# All ingresses
kubectl get ingress -A -l app.kubernetes.io/part-of=mechanicbuddy
```

#### View resource consumption by namespace
```bash
kubectl top pods -A -l app.kubernetes.io/part-of=mechanicbuddy --containers
```

### Troubleshooting

#### Free-tier tenants not routing correctly
```bash
# Check ingress configuration
kubectl describe ingress -n mechanicbuddy-free-tier

# Check API can resolve tenants
kubectl logs -n mechanicbuddy-free-tier -l app.kubernetes.io/component=api | grep "X-Tenant-ID"

# Verify database connectivity
kubectl exec -n mechanicbuddy-free-tier deployment/free-tier-api -- \
  curl -s http://localhost:5000/health
```

#### Tenant deployment stuck
```bash
# Check provisioning job logs
kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=management-api | grep provisioning

# Check tenant namespace events
kubectl get events -n tenant-{id} --sort-by='.lastTimestamp'

# Check migration job
kubectl get jobs -n tenant-{id}
kubectl logs -n tenant-{id} job/db-migration
```

#### Certificate issues
```bash
# Check certificate status
kubectl get certificate -A -l app.kubernetes.io/part-of=mechanicbuddy

# Describe certificate for details
kubectl describe certificate -n tenant-{id} {tenant-id}-tls

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager
```

## Security

### Isolation Levels

**Free-Tier Tenants**:
- Schema-level isolation in shared database
- Multi-tenant middleware enforces tenant boundaries
- Shared compute resources with fair-use policies
- Shared ingress with hostname-based routing

**Paid-Tier Tenants**:
- Namespace-level isolation with NetworkPolicy
- Separate PostgreSQL instance per tenant
- Dedicated compute resources
- Individual JWT secrets per tenant
- Separate ingress and TLS certificates

### Security Features

- TLS certificates auto-provisioned via cert-manager
- Secrets encrypted at rest (Kubernetes secrets)
- Super admin access logged for audit
- Rate limiting per tenant
- DDoS protection via ingress-nginx
- Database credentials rotated on provisioning
- Network policies restrict inter-namespace communication

## Upgrades and Migrations

### Upgrade System Chart

```bash
# Pull latest charts
git pull origin main

# Review changes
helm diff upgrade mechanicbuddy-system ./charts/mechanicbuddy-system \
  --namespace mechanicbuddy-system \
  -f values-secrets.yaml

# Upgrade
helm upgrade mechanicbuddy-system ./charts/mechanicbuddy-system \
  --namespace mechanicbuddy-system \
  -f values-secrets.yaml

# Verify upgrade
kubectl rollout status deployment/management-api -n mechanicbuddy-system
```

### Upgrade Free-Tier Chart

Free-tier upgrades are handled automatically by ArgoCD:

```bash
# Check ArgoCD sync status
kubectl get application mechanicbuddy-free-tier -n argocd

# Force sync if needed
kubectl patch application mechanicbuddy-free-tier -n argocd \
  --type merge -p '{"operation":{"initiatedBy":{"username":"admin"},"sync":{"revision":"main"}}}'

# Monitor rollout
kubectl rollout status deployment/free-tier-api -n mechanicbuddy-free-tier
kubectl rollout status deployment/free-tier-web -n mechanicbuddy-free-tier
```

### Upgrade Individual Tenant

Tenant upgrades are typically managed by the Management API, but can be done manually:

```bash
# Upgrade specific tenant
helm upgrade tenant-{id} ./charts/mechanicbuddy-tenant \
  --namespace tenant-{id} \
  --reuse-values \
  --set image.tag=v1.2.3

# Monitor rollout
kubectl rollout status deployment/api -n tenant-{id}
kubectl rollout status deployment/web -n tenant-{id}
```

### Bulk Tenant Upgrades

Upgrade all paid tenants to a new version:

```bash
#!/bin/bash
NEW_VERSION="v1.2.3"

# Get all tenant namespaces
TENANTS=$(kubectl get namespaces -l mechanicbuddy.app/namespace-type=tenant -o jsonpath='{.items[*].metadata.name}')

for tenant in $TENANTS; do
  echo "Upgrading $tenant to $NEW_VERSION"
  helm upgrade $(echo $tenant | sed 's/tenant-//') ./charts/mechanicbuddy-tenant \
    --namespace $tenant \
    --reuse-values \
    --set image.tag=$NEW_VERSION \
    --wait
done
```

### Migrate Tenant Between Tiers

When a tenant upgrades from free to paid tier:

```bash
# 1. Management API automatically:
#    - Creates new namespace
#    - Deploys dedicated resources
#    - Migrates data from shared to dedicated database
#    - Updates DNS/ingress routing
#    - Updates tenant metadata

# 2. Monitor migration progress
kubectl get events -n tenant-{id} --sort-by='.lastTimestamp'
kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=management-api | grep "migration.*{tenant-id}"

# 3. Verify new deployment
kubectl get pods -n tenant-{id}
kubectl get cluster -n tenant-{id}
```

### Rollback

If an upgrade fails:

```bash
# Rollback system chart
helm rollback mechanicbuddy-system -n mechanicbuddy-system

# Rollback free-tier (via ArgoCD)
kubectl patch application mechanicbuddy-free-tier -n argocd \
  --type merge -p '{"spec":{"source":{"targetRevision":"previous-commit-hash"}}}'

# Rollback specific tenant
helm rollback tenant-{id} -n tenant-{id}

# Or use kubectl rollout
kubectl rollout undo deployment/api -n tenant-{id}
```

## Best Practices

### Capacity Planning

**Free-Tier**:
- Monitor tenant count: Should not exceed configured `maxTenants` (default: 100)
- Monitor resource usage: Scale up if CPU/memory consistently above 70%
- Plan for burst capacity: Ensure HPA can scale to 10+ replicas

**Paid Tenants**:
- Review resource allocation quarterly based on actual usage
- Consider node affinity/anti-affinity for large enterprise tenants
- Plan storage growth: Database backups, work order history, etc.

### Monitoring

Set up monitoring and alerting for:

- Free-tier replica count (alert if at max for extended period)
- Free-tier tenant count (alert when approaching maxTenants)
- Tenant deployment failures (alert on failed provisioning jobs)
- Database connection pool exhaustion
- Certificate expiration (should auto-renew, but monitor)
- Storage usage per tenant database

### Backup Strategy

**System Database**:
```bash
# Automated backups configured in system chart
kubectl get backup -n mechanicbuddy-system

# Manual backup
kubectl cnpg backup mechanicbuddy-system-postgresql -n mechanicbuddy-system
```

**Tenant Databases**:
```bash
# Check backup schedule
kubectl get cluster tenant-{id}-postgresql -n tenant-{id} -o jsonpath='{.spec.backup}'

# Manual backup
kubectl cnpg backup tenant-{id}-postgresql -n tenant-{id}
```

### Cost Optimization

- Use cluster autoscaler for dynamic node provisioning
- Configure PodDisruptionBudgets for graceful node draining
- Use node pools with different instance types (free-tier vs paid tiers)
- Monitor storage costs: Implement data retention policies
- Consider regional pricing for multi-region deployments

## Support

For issues or questions:

- Documentation: `/infrastructure/helm/charts/*/README.md`
- Management API logs: `kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=management-api`
- Platform metrics: Access via Grafana/Prometheus (if configured)
- GitHub Issues: https://github.com/your-org/mechanicbuddy/issues
