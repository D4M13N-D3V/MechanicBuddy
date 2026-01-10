# MechanicBuddy Free-Tier Helm Chart

Comprehensive documentation for the MechanicBuddy shared free-tier deployment.

## Purpose

The `mechanicbuddy-free-tier` Helm chart deploys a **shared multi-tenant infrastructure** for all free and demo tier tenants. This approach provides:

- **Cost-efficient alternative** to per-tenant deployments
- **Resource optimization** through shared API and Web services
- **Scalability** supporting up to 100 tenants per shared instance
- **Database-level tenant isolation** using separate databases on a shared PostgreSQL cluster
- **Simplified management** with centralized monitoring and updates

This is ideal for users who require basic functionality without the overhead of dedicated infrastructure.

## Architecture

### Deployment Overview

**Namespace:** `mechanicbuddy-free-tier`

The free-tier instance consists of the following components:

#### Core Services
- **API Deployment** (2 replicas)
  - .NET 9 ASP.NET Core Web API
  - Multi-tenancy enabled via JWT claims
  - Routes each request to the correct tenant database
  - Health checks: `/health/live` and `/health/ready`

- **Web Deployment** (2 replicas)
  - Next.js 15 frontend application
  - Server-side rendering with App Router
  - Shared UI serving all free-tier tenants

- **Service Account & RBAC**
  - Dedicated service account for pod security
  - Minimal required permissions

- **Resource Quotas**
  - Namespace-level limits to prevent resource exhaustion
  - Configurable CPU, memory, and pod counts

#### External Dependencies

- **PostgreSQL Database**
  - Host: `mechanicbuddy-management-db-rw.mechanicbuddy-system.svc.cluster.local`
  - Port: `5432`
  - Uses the shared management database cluster
  - Each tenant gets a separate database: `mechanicbuddy-{tenantId}`

- **Nginx Proxy Manager (NPM)**
  - Handles external routing and SSL termination
  - Routes traffic based on hostname to the shared services
  - Manages Let's Encrypt certificates

- **Email (Resend SMTP)**
  - Host: `smtp.resend.com`
  - Port: `587`
  - From address: `noreply@mechanicbuddy.app`

### Tenant Isolation Strategy

Multi-tenancy is implemented through multiple isolation layers:

#### 1. Database-Level Isolation
- Each tenant receives a dedicated database on the shared PostgreSQL cluster
- Database naming: `mechanicbuddy-{tenantId}`
- Complete schema isolation between tenants
- Migrations run automatically for the template database

#### 2. Application-Level Isolation
- JWT claims contain tenant identifier
- API resolves tenant from JWT on each request
- Database connection routing based on tenant ID
- No cross-tenant data access possible

#### 3. Network-Level Architecture
- Shared API and Web services (cost optimization)
- Isolated database instances (security)
- External routing via Nginx Proxy Manager
- Traffic routing based on hostname

## Configuration

### Default Values (`values.yaml`)

The chart includes sensible defaults for a shared free-tier deployment:

```yaml
# Instance Configuration
instance:
  id: "free-tier"
  namespace: "mechanicbuddy-free-tier"

# Domain Configuration
domains:
  baseDomain: "mechanicbuddy.app"
  clusterIssuer: "letsencrypt-prod"

# PostgreSQL - External Cluster
postgresql:
  host: "mechanicbuddy-management-db-rw.mechanicbuddy-system.svc.cluster.local"
  port: 5432
  database: "mechanicbuddy"
  username: "management"
  password: ""  # MUST be provided

# API Service Configuration
api:
  replicas: 2
  image:
    repository: "ghcr.io/d4m13n-d3v/mechanicbuddy-api"
    tag: "latest"
    pullPolicy: "IfNotPresent"
  port: 15567
  resources:
    requests:
      memory: "512Mi"
      cpu: "250m"
    limits:
      memory: "1Gi"
      cpu: "1000m"
  healthCheck:
    liveness: "/health/live"
    readiness: "/health/ready"

# Web Service Configuration
web:
  replicas: 2
  image:
    repository: "ghcr.io/d4m13n-d3v/mechanicbuddy-web"
    tag: "latest"
    pullPolicy: "IfNotPresent"
  port: 3000
  resources:
    requests:
      memory: "256Mi"
      cpu: "100m"
    limits:
      memory: "512Mi"
      cpu: "500m"

# Database Migrations
migrations:
  enabled: true
  image:
    repository: "ghcr.io/d4m13n-d3v/mechanicbuddy-dbup"
    tag: "latest"
    pullPolicy: "IfNotPresent"
  timeout: 300

# Email Configuration
email:
  host: "smtp.resend.com"
  port: 587
  apiKey: ""  # MUST be provided
  fromAddress: "noreply@mechanicbuddy.app"

# Security Secrets (auto-generated if not provided)
secrets:
  jwtSecret: ""
  consumerSecret: ""
  sessionSecret: ""

# Resource Quotas
resourceQuota:
  enabled: true
  requests:
    cpu: "4"
    memory: "8Gi"
  limits:
    cpu: "8"
    memory: "16Gi"
  pods: "20"

# Security Context
securityContext:
  runAsNonRoot: true
  runAsUser: 1000
  fsGroup: 1000

# Service Account
serviceAccount:
  create: true
  name: ""
  annotations: {}
```

### Production Overrides (`values-production.yaml`)

Production deployments use specific image tags and configurations:

```yaml
# Use commit SHA tags for reproducible deployments
api:
  image:
    tag: "0f5f73e"  # Updated by CI/CD

web:
  image:
    tag: "0f5f73e"  # Updated by CI/CD

migrations:
  image:
    tag: "0f5f73e"  # Updated by CI/CD

# PostgreSQL credentials
postgresql:
  password: "ProductionDBPassword456!"

# Image pull secrets for private registry
imagePullSecrets:
  - name: ghcr-credentials

# Resource quotas disabled (managed by ArgoCD project)
resourceQuota:
  enabled: false
```

## Installation

### Prerequisites

Before installing the chart, ensure:

1. Kubernetes cluster is running (1.25+)
2. Helm is installed (3.10+)
3. PostgreSQL management cluster exists in `mechanicbuddy-system` namespace
4. Image pull secrets configured (for private registries)
5. Nginx Proxy Manager configured for external routing

### Manual Installation

```bash
# Add GHCR credentials (if using private registry)
kubectl create secret docker-registry ghcr-credentials \
  --docker-server=ghcr.io \
  --docker-username=<github-username> \
  --docker-password=<github-token> \
  --namespace mechanicbuddy-free-tier

# Install the chart
helm install mechanicbuddy-free-tier \
  ./infrastructure/helm/charts/mechanicbuddy-free-tier \
  --namespace mechanicbuddy-free-tier \
  --create-namespace \
  -f ./infrastructure/helm/charts/mechanicbuddy-free-tier/values-production.yaml \
  --set postgresql.password='YourSecurePassword' \
  --set email.apiKey='your-resend-api-key'

# Verify deployment
kubectl get pods -n mechanicbuddy-free-tier
kubectl get services -n mechanicbuddy-free-tier
```

### Upgrade Existing Deployment

```bash
helm upgrade mechanicbuddy-free-tier \
  ./infrastructure/helm/charts/mechanicbuddy-free-tier \
  --namespace mechanicbuddy-free-tier \
  -f ./infrastructure/helm/charts/mechanicbuddy-free-tier/values-production.yaml \
  --set postgresql.password='YourSecurePassword' \
  --set email.apiKey='your-resend-api-key'
```

### Uninstallation

```bash
# Remove the Helm release
helm uninstall mechanicbuddy-free-tier --namespace mechanicbuddy-free-tier

# Clean up namespace (optional)
kubectl delete namespace mechanicbuddy-free-tier
```

## ArgoCD Deployment

The free-tier instance is managed by ArgoCD for GitOps-based continuous deployment.

### ArgoCD Application

**Location:** `/infrastructure/argocd/apps/free-tier-production.yaml`

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: mechanicbuddy-free-tier
  namespace: argocd
spec:
  project: mechanicbuddy

  source:
    repoURL: https://github.com/D4m13n-D3v/mechanicbuddy.git
    targetRevision: main
    path: infrastructure/helm/charts/mechanicbuddy-free-tier
    helm:
      valueFiles:
        - values.yaml
        - values-production.yaml

  destination:
    server: https://kubernetes.default.svc
    namespace: mechanicbuddy-free-tier

  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
      - PrunePropagationPolicy=foreground
      - PruneLast=true
```

### Key Features

- **Auto-sync enabled:** Changes to `main` branch automatically deploy
- **Self-healing enabled:** Drift from Git state is automatically corrected
- **Automated pruning:** Removed resources are cleaned up
- **Retry logic:** Failed syncs retry with exponential backoff
- **Namespace creation:** Namespace created automatically if missing

### Deployment Workflow

1. Developer commits changes to `main` branch
2. GitHub Actions builds and pushes Docker images
3. Image tags updated in `values-production.yaml`
4. ArgoCD detects Git changes within 3 minutes
5. ArgoCD syncs the Helm chart to cluster
6. Kubernetes performs rolling update of pods
7. Health checks verify new pods are healthy

### Managing via ArgoCD UI

```bash
# Access ArgoCD UI (port-forward if needed)
kubectl port-forward svc/argocd-server -n argocd 8080:443

# View application status
# Navigate to: https://localhost:8080/applications/mechanicbuddy-free-tier
```

### Manual Sync (if needed)

```bash
# Trigger manual sync
argocd app sync mechanicbuddy-free-tier

# View sync status
argocd app get mechanicbuddy-free-tier

# View application logs
argocd app logs mechanicbuddy-free-tier
```

## Scaling Considerations

### Horizontal Scaling

The shared instance supports scaling based on tenant count and resource utilization:

#### Replica Scaling

```bash
# Scale API replicas
kubectl scale deployment mechanicbuddy-free-tier-api \
  -n mechanicbuddy-free-tier \
  --replicas=4

# Scale Web replicas
kubectl scale deployment mechanicbuddy-free-tier-web \
  -n mechanicbuddy-free-tier \
  --replicas=4
```

Or update via Helm values:

```yaml
api:
  replicas: 4

web:
  replicas: 4
```

#### Resource Adjustments

For higher tenant counts, increase resource limits:

```yaml
api:
  resources:
    requests:
      memory: "1Gi"
      cpu: "500m"
    limits:
      memory: "2Gi"
      cpu: "2000m"

web:
  resources:
    requests:
      memory: "512Mi"
      cpu: "200m"
    limits:
      memory: "1Gi"
      cpu: "1000m"
```

### Vertical Scaling

When reaching capacity limits (100 tenants per instance):

1. **Deploy additional shared instances:**
   ```bash
   # Deploy free-tier-2 instance
   helm install mechanicbuddy-free-tier-2 \
     ./infrastructure/helm/charts/mechanicbuddy-free-tier \
     --namespace mechanicbuddy-free-tier-2 \
     --set instance.id=free-tier-2
   ```

2. **Load balance tenants** across multiple shared instances
3. **Monitor database connections** on PostgreSQL cluster
4. **Consider PostgreSQL read replicas** for read-heavy workloads

### Monitoring Metrics

Key metrics to monitor for scaling decisions:

- **Tenant count:** Maximum 100 per instance recommended
- **CPU utilization:** Scale when consistently above 70%
- **Memory usage:** Scale when consistently above 80%
- **Database connections:** Monitor active connections per tenant
- **Response latency:** P95 latency should stay below 500ms
- **Pod restarts:** Frequent restarts indicate resource pressure

### Database Scaling

PostgreSQL cluster considerations:

```yaml
# Monitor these metrics on the management cluster:
- Total databases (one per tenant)
- Connection pool usage
- Query performance
- Storage capacity
- Backup frequency
```

When approaching limits:
- Increase PostgreSQL cluster resources
- Add read replicas for read-heavy workloads
- Consider connection pooling (PgBouncer)
- Archive inactive tenant databases

## Troubleshooting

### Common Issues

#### 1. Pods Not Starting

```bash
# Check pod status
kubectl get pods -n mechanicbuddy-free-tier

# View pod logs
kubectl logs -n mechanicbuddy-free-tier deployment/mechanicbuddy-free-tier-api
kubectl logs -n mechanicbuddy-free-tier deployment/mechanicbuddy-free-tier-web

# Describe pod for events
kubectl describe pod <pod-name> -n mechanicbuddy-free-tier
```

Common causes:
- Image pull failures (check `imagePullSecrets`)
- Database connection issues (verify PostgreSQL connectivity)
- Missing secrets (check secret configuration)
- Resource limits too low

#### 2. Database Connection Failures

```bash
# Test PostgreSQL connectivity from API pod
kubectl exec -n mechanicbuddy-free-tier deployment/mechanicbuddy-free-tier-api -- \
  nc -zv mechanicbuddy-management-db-rw.mechanicbuddy-system.svc.cluster.local 5432

# Check database credentials
kubectl get secret -n mechanicbuddy-free-tier mechanicbuddy-free-tier-secrets -o yaml
```

#### 3. High Memory Usage

```bash
# Check current resource usage
kubectl top pods -n mechanicbuddy-free-tier

# View resource limits
kubectl describe deployment -n mechanicbuddy-free-tier
```

Solutions:
- Increase memory limits in `values.yaml`
- Add more replicas to distribute load
- Check for memory leaks in application logs

#### 4. ArgoCD Sync Failures

```bash
# View sync status
argocd app get mechanicbuddy-free-tier

# Check sync logs
argocd app logs mechanicbuddy-free-tier --tail 100
```

Common causes:
- Invalid Helm values
- Resource quota exceeded
- RBAC permission issues

### Health Checks

Verify system health:

```bash
# API health
kubectl exec -n mechanicbuddy-free-tier deployment/mechanicbuddy-free-tier-api -- \
  curl -f http://localhost:15567/health/live

# Check all resources
kubectl get all -n mechanicbuddy-free-tier

# View recent events
kubectl get events -n mechanicbuddy-free-tier --sort-by='.lastTimestamp'
```

## Security

### Secrets Management

Sensitive data is stored in Kubernetes secrets:

```yaml
# Auto-generated secrets (if not provided)
secrets:
  jwtSecret: ""        # Used for JWT token signing
  consumerSecret: ""   # API consumer authentication
  sessionSecret: ""    # Session cookie encryption
```

**Best Practices:**
- Use external secret management (e.g., Sealed Secrets, External Secrets Operator)
- Rotate secrets regularly
- Never commit secrets to Git
- Use ArgoCD secret plugins for production

### Pod Security

```yaml
securityContext:
  runAsNonRoot: true  # Prevents root execution
  runAsUser: 1000     # Non-privileged user
  fsGroup: 1000       # File system group
```

### Network Policies

Consider implementing network policies for additional isolation:

```yaml
# Example: Restrict API to only accept traffic from Web
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: api-network-policy
spec:
  podSelector:
    matchLabels:
      app: mechanicbuddy-free-tier-api
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: mechanicbuddy-free-tier-web
```

## Support

For issues, questions, or contributions:

- **Email:** support@mechanicbuddy.app
- **Repository:** https://github.com/D4m13n-D3v/mechanicbuddy
- **Documentation:** See `/infrastructure/helm/README.md` for general Helm documentation

## License

See the main repository for license information.
