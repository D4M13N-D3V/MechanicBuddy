# MechanicBuddy Tenant Helm Chart

Deploy an isolated MechanicBuddy instance for a tenant.

## Prerequisites

- Kubernetes 1.29+
- Helm 3.0+
- CloudNativePG operator installed
- ingress-nginx controller installed
- cert-manager installed with ClusterIssuer

## Installation

### Quick Start (Demo)

```bash
helm install demo-tenant ./mechanicbuddy-tenant \
  --set tenant.id=demo123 \
  --set tenant.name="Demo Workshop" \
  --set tenant.ownerEmail="demo@example.com" \
  -f values/demo.yaml
```

### Production Deployment

```bash
helm install acme-workshop ./mechanicbuddy-tenant \
  --set tenant.id=acme \
  --set tenant.name="ACME Auto Repair" \
  --set tenant.ownerEmail="admin@acme.com" \
  --set tenant.tier=professional \
  --set billing.stripeCustomerId=cus_xxx \
  -f values/professional.yaml
```

### With Custom Domain

```bash
helm install acme-workshop ./mechanicbuddy-tenant \
  --set tenant.id=acme \
  --set tenant.name="ACME Auto Repair" \
  --set domains.custom[0]=workshop.acme.com \
  -f values/professional.yaml
```

## Configuration

### Tenant Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `tenant.id` | Unique tenant identifier | `""` (required) |
| `tenant.name` | Company display name | `""` |
| `tenant.tier` | Subscription tier (demo/free/professional) | `"free"` |
| `tenant.ownerEmail` | Owner email for notifications | `""` |

### Domain Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `domains.baseDomain` | Base domain for the platform | `"mechanicbuddy.app"` |
| `domains.default` | Default subdomain | `"{tenant.id}.mechanicbuddy.app"` |
| `domains.custom` | List of custom domains | `[]` |
| `domains.clusterIssuer` | cert-manager ClusterIssuer | `"letsencrypt-prod"` |

### Demo/Trial Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `demo.enabled` | Enable demo mode | `false` |
| `demo.expirationDays` | Trial duration in days | `7` |
| `demo.populateSampleData` | Add sample data | `true` |

### PostgreSQL Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `postgresql.instances` | Number of replicas | `1` |
| `postgresql.database` | Database name | `"mechanicbuddy"` |
| `postgresql.storage.size` | Storage size | `"10Gi"` |
| `postgresql.storage.storageClass` | Storage class | `"local-path"` |

### API Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `api.replicas` | Number of API replicas | `1` |
| `api.image.repository` | API image | `"ghcr.io/mechanicbuddy/api"` |
| `api.image.tag` | Image tag | `"latest"` |
| `api.resources` | Resource requests/limits | See values.yaml |

### Web Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `web.replicas` | Number of web replicas | `1` |
| `web.image.repository` | Web image | `"ghcr.io/mechanicbuddy/web"` |
| `web.image.tag` | Image tag | `"latest"` |
| `web.resources` | Resource requests/limits | See values.yaml |

### Billing Settings

| Parameter | Description | Default |
|-----------|-------------|---------|
| `billing.stripeCustomerId` | Stripe customer ID | `""` |
| `billing.subscriptionId` | Stripe subscription ID | `""` |
| `billing.mechanicLimit` | Max mechanics (null=unlimited) | `null` |

## Tiers

| Tier | Mechanics | Features |
|------|-----------|----------|
| demo | 3 | 7-day trial, sample data |
| free | 1 | Basic features |
| professional | unlimited | Full features, backups |

## Architecture

Each tenant deployment includes:

```
tenant-{id} namespace
├── PostgreSQL Cluster (CloudNativePG)
├── API Deployment + Service
├── Web Deployment + Service
├── Ingress with TLS
├── ConfigMap (configuration)
├── Secrets (credentials)
├── NetworkPolicy (isolation)
└── ServiceAccount
```

## Upgrade

```bash
helm upgrade acme-workshop ./mechanicbuddy-tenant \
  --set tenant.id=acme \
  -f values/professional.yaml
```

## Uninstall

```bash
helm uninstall acme-workshop
kubectl delete namespace tenant-acme
```

## Troubleshooting

### Check pod status

```bash
kubectl get pods -n tenant-{id}
```

### Check PostgreSQL status

```bash
kubectl get cluster -n tenant-{id}
```

### View logs

```bash
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=api
kubectl logs -n tenant-{id} -l app.kubernetes.io/component=web
```

### Check certificate status

```bash
kubectl get certificate -n tenant-{id}
```
