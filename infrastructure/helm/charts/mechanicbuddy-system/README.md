# MechanicBuddy System Helm Chart

Deploy the MechanicBuddy management system (admin portal, billing, tenant provisioning).

## Prerequisites

- Kubernetes 1.29+
- Helm 3.0+
- CloudNativePG operator installed
- ingress-nginx controller installed
- cert-manager installed with ClusterIssuer
- Stripe account configured
- Resend account for email

## Installation

### Configure Secrets

Create a `values-secrets.yaml` file (don't commit to git):

```yaml
stripe:
  publishableKey: "pk_live_xxx"
  secretKey: "sk_live_xxx"
  webhookSecret: "whsec_xxx"
  prices:
    free: "price_xxx"
    standard: "price_xxx"
    growth: "price_xxx"
    scale: "price_xxx"

email:
  apiKey: "re_xxx"

cloudflare:
  enabled: true
  apiToken: "xxx"
  zoneId: "xxx"

superAdmin:
  initialEmail: "admin@mechanicbuddy.app"
  initialPassword: "secure-password-here"
```

### Install

```bash
helm install mechanicbuddy-system ./mechanicbuddy-system \
  -f values-secrets.yaml \
  --set global.domain=mechanicbuddy.app
```

## Configuration

### Global Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `global.domain` | Base domain | `"mechanicbuddy.app"` |
| `global.imagePullSecrets` | Image pull secrets | `[]` |

### Stripe Configuration

| Parameter | Description | Default |
|-----------|-------------|---------|
| `stripe.publishableKey` | Stripe publishable key | `""` |
| `stripe.secretKey` | Stripe secret key | `""` |
| `stripe.webhookSecret` | Stripe webhook secret | `""` |
| `stripe.prices.free` | Free tier price ID | `""` |
| `stripe.prices.standard` | Standard tier price ID ($20) | `""` |
| `stripe.prices.growth` | Growth tier price ID ($15) | `""` |
| `stripe.prices.scale` | Scale tier price ID ($10) | `""` |

### Email Configuration

| Parameter | Description | Default |
|-----------|-------------|---------|
| `email.provider` | Email provider | `"resend"` |
| `email.apiKey` | API key | `""` |
| `email.fromAddress` | Sender address | `"noreply@mechanicbuddy.app"` |

### Super Admin

| Parameter | Description | Default |
|-----------|-------------|---------|
| `superAdmin.initialEmail` | Initial admin email | `"admin@mechanicbuddy.app"` |
| `superAdmin.initialPassword` | Initial admin password | Auto-generated |

## Architecture

```
mechanicbuddy-system namespace
├── PostgreSQL Cluster (management database)
├── Management API Deployment
├── Management Portal Deployment
├── Ingress (mechanicbuddy.app, api.mechanicbuddy.app)
├── CronJob: Metrics Collector (every 4 hours)
├── CronJob: Demo Cleanup (daily)
├── ServiceAccount with ClusterRole
└── Secrets & ConfigMaps
```

## RBAC

The Management API needs cluster-wide permissions to:
- Create/delete tenant namespaces
- Deploy tenant resources (PostgreSQL, Deployments, Services, Ingress)
- Manage certificates

## Stripe Webhook Setup

Configure your Stripe webhook to point to:
```
https://api.mechanicbuddy.app/webhooks/stripe
```

Events to subscribe:
- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.paid`
- `invoice.payment_failed`

## Upgrade

```bash
helm upgrade mechanicbuddy-system ./mechanicbuddy-system \
  -f values-secrets.yaml
```

## Troubleshooting

### Check pods
```bash
kubectl get pods -n mechanicbuddy-system
```

### Check logs
```bash
kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=management-api
kubectl logs -n mechanicbuddy-system -l app.kubernetes.io/component=portal
```

### Check CronJobs
```bash
kubectl get cronjobs -n mechanicbuddy-system
kubectl get jobs -n mechanicbuddy-system
```
