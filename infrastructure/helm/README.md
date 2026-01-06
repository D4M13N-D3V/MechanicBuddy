# MechanicBuddy Helm Charts

Helm charts for deploying MechanicBuddy SaaS platform on Kubernetes.

## Charts

| Chart | Description |
|-------|-------------|
| `mechanicbuddy-system` | Management portal, API, billing, tenant provisioning |
| `mechanicbuddy-tenant` | Individual tenant deployment (per-customer instance) |

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

## Quick Start

### 1. Deploy Management System

```bash
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
EOF

# Install
helm install mechanicbuddy-system ./charts/mechanicbuddy-system \
  -f values-secrets.yaml \
  --set global.domain=mechanicbuddy.app
```

### 2. Deploy a Tenant (Demo)

```bash
helm install demo-tenant ./charts/mechanicbuddy-tenant \
  --set tenant.id=demo123 \
  --set tenant.name="Demo Workshop" \
  --set tenant.ownerEmail="demo@example.com" \
  -f ./charts/mechanicbuddy-tenant/values/demo.yaml
```

### 3. Deploy a Tenant (Production)

```bash
helm install acme-workshop ./charts/mechanicbuddy-tenant \
  --set tenant.id=acme \
  --set tenant.name="ACME Auto Repair" \
  --set tenant.ownerEmail="admin@acme.com" \
  --set billing.stripeCustomerId=cus_xxx \
  -f ./charts/mechanicbuddy-tenant/values/professional.yaml
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         KUBERNETES CLUSTER                          │
├─────────────────────────────────────────────────────────────────────┤
│  mechanicbuddy-system namespace                                     │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │ Management Portal│  │ Management API   │  │ Management DB    │  │
│  │ (Next.js)        │  │ (.NET 9)         │  │ (PostgreSQL)     │  │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘  │
│                                                                     │
│  Shared Services: ingress-nginx, cert-manager, CloudNativePG       │
├─────────────────────────────────────────────────────────────────────┤
│  tenant-{id} namespace (per customer)                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │ Frontend     │  │ API          │  │ PostgreSQL   │              │
│  │ (Next.js)    │  │ (.NET 9)     │  │ (isolated)   │              │
│  └──────────────┘  └──────────────┘  └──────────────┘              │
│                                                                     │
│  Custom domain: workshop.customerdomain.com                         │
│  Default domain: {tenant-id}.mechanicbuddy.app                      │
└─────────────────────────────────────────────────────────────────────┘
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

| Tier | Mechanics | Price |
|------|-----------|-------|
| Free | 1 | $0/month |
| Standard | 2-10 | $20/mechanic/month |
| Growth | 11-20 | $15/mechanic/month |
| Scale | 21+ | $10/mechanic/month |

Demo: 7-day free trial with sample data.

## Maintenance

### List all tenants
```bash
kubectl get namespaces -l mechanicbuddy.app/namespace-type=tenant
```

### Check tenant status
```bash
kubectl get pods -n tenant-{id}
kubectl get cluster -n tenant-{id}  # PostgreSQL
kubectl get ingress -n tenant-{id}
kubectl get certificate -n tenant-{id}
```

### View tenant logs
```bash
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=api
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=web
```

### Delete a tenant
```bash
helm uninstall tenant-{id}
kubectl delete namespace tenant-{id}
```

## Security

- Each tenant has isolated namespace with NetworkPolicy
- Separate PostgreSQL instance per tenant
- JWT secrets generated per tenant
- TLS certificates auto-provisioned via cert-manager
- Super admin access logged for audit
