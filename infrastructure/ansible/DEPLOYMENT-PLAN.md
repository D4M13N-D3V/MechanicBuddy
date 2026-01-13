# MechanicBuddy Production Deployment Plan

This document outlines a comprehensive, automated deployment strategy for MechanicBuddy on the RKE2 Kubernetes cluster with modern secrets management.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           SECRETS MANAGEMENT                                 │
│  ┌─────────────┐     ┌──────────────────┐     ┌─────────────────────────┐  │
│  │  Bitwarden  │────▶│ External Secrets │────▶│   Kubernetes Secrets    │  │
│  │  Secrets    │     │    Operator      │     │  (auto-synced)          │  │
│  │  Manager    │     └──────────────────┘     └─────────────────────────┘  │
│  └─────────────┘                                                            │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                           CLUSTER COMPONENTS                                 │
│                                                                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐│
│  │   NGINX     │  │    cert-    │  │ CloudNative │  │  External Secrets   ││
│  │   Ingress   │  │   manager   │  │     PG      │  │     Operator        ││
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────────────┘│
│                                                                              │
│  ┌──────────────────────────────┐  ┌──────────────────────────────────────┐│
│  │   mechanicbuddy-system       │  │   mechanicbuddy-free-tier            ││
│  │   - Management API           │  │   - Shared API (multi-tenant)        ││
│  │   - Management Portal        │  │   - Shared Web (multi-tenant)        ││
│  │   - PostgreSQL               │  │   - PostgreSQL per tenant            ││
│  └──────────────────────────────┘  └──────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────────┘
```

## Secrets Management Strategy

### Option A: External Secrets Operator + Bitwarden (Recommended)

**Pros:**
- Secrets stored centrally in Bitwarden Secrets Manager
- Automatic sync to Kubernetes (configurable interval)
- GitOps-friendly (ExternalSecret manifests in git, actual secrets in Bitwarden)
- Single source of truth
- Audit trail in Bitwarden

**Components:**
1. External Secrets Operator (ESO)
2. Bitwarden SDK Server (sidecar)
3. Bitwarden Secrets Manager subscription

### Option B: Bitwarden Native Operator

**Pros:**
- Official Bitwarden operator
- Simpler setup (no ESO dependency)

**Cons:**
- Less flexible than ESO
- Fewer provider options if you switch later

### Option C: Sealed Secrets (GitOps Alternative)

**Pros:**
- Encrypted secrets can be committed to git
- No external dependencies

**Cons:**
- Key rotation more complex
- No central UI for management

---

## Bitwarden Secrets Manager Setup

### Prerequisites

1. **Bitwarden Organization** with Secrets Manager enabled
2. **Machine Account** with access token
3. **Project** to organize secrets

### Secrets Structure in Bitwarden

Create a project called `mechanicbuddy-production` with these secrets:

```
mechanicbuddy-production/
├── infrastructure/
│   ├── ghcr-username          # GitHub Container Registry username
│   ├── ghcr-token             # GitHub PAT for image pulls
│   ├── cloudflare-api-token   # For DNS automation
│   └── cloudflare-zone-id     # Zone ID for mechanicbuddy.app
│
├── database/
│   ├── postgres-password      # Main PostgreSQL superuser password
│   ├── management-db-password # Management database password
│   └── free-tier-db-password  # Free-tier database password
│
├── application/
│   ├── jwt-secret             # JWT signing secret (64+ chars)
│   ├── consumer-secret        # API consumer secret
│   ├── session-secret         # Next.js session encryption
│   ├── stripe-secret-key      # Stripe API secret
│   ├── stripe-webhook-secret  # Stripe webhook signing secret
│   ├── stripe-publishable-key # Stripe public key
│   └── resend-api-key         # Resend email API key
│
└── admin/
    └── initial-admin-password # Initial super admin password
```

---

## Deployment Phases

### Phase 1: Core Infrastructure (Automated via Ansible)

**Duration:** ~15 minutes

| Component | Version | Purpose |
|-----------|---------|---------|
| NGINX Ingress | 4.9.x | Load balancer + ingress |
| cert-manager | 1.14.x | TLS certificate management |
| CloudNativePG | 1.22.x | PostgreSQL operator |
| External Secrets Operator | 0.9.x | Secrets sync from Bitwarden |
| local-path-provisioner | (RKE2 built-in) | Storage class |

**Run:**
```bash
ansible-playbook playbooks/rke2-02-install-addons.yml --vault-password-file .vault_pass
```

### Phase 2: Secrets Management Setup

**Duration:** ~10 minutes

1. Create Bitwarden machine account and access token
2. Store all secrets in Bitwarden project
3. Deploy SecretStore and ExternalSecret resources

**Run:**
```bash
# First, set your Bitwarden access token
export BITWARDEN_ACCESS_TOKEN="your-token-here"

ansible-playbook playbooks/rke2-03-setup-secrets.yml --vault-password-file .vault_pass
```

### Phase 3: Deploy Applications

**Duration:** ~10 minutes

1. Deploy mechanicbuddy-system (Management)
2. Deploy mechanicbuddy-free-tier (Tenant hosting)
3. Run database migrations
4. Configure DNS

**Run:**
```bash
ansible-playbook playbooks/rke2-04-deploy-apps.yml --vault-password-file .vault_pass
```

---

## Playbook Details

### rke2-02-install-addons.yml

Installs all cluster addons:
- Helm repositories
- NGINX Ingress Controller (with LoadBalancer or NodePort)
- cert-manager + ClusterIssuers (Let's Encrypt)
- CloudNativePG operator
- External Secrets Operator + Bitwarden SDK Server

### rke2-03-setup-secrets.yml

Sets up secrets management:
- Creates namespaces
- Deploys Bitwarden authentication secret
- Creates SecretStore resources
- Creates ExternalSecret resources for all apps

### rke2-04-deploy-apps.yml

Deploys MechanicBuddy:
- Installs mechanicbuddy-system Helm chart
- Installs mechanicbuddy-free-tier Helm chart
- Waits for PostgreSQL clusters
- Runs database migrations
- Configures Cloudflare DNS (optional)

---

## Secret Mappings

### External Secrets Configuration

```yaml
# Example: API Secrets ExternalSecret
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: api-secrets
  namespace: mechanicbuddy-free-tier
spec:
  refreshInterval: 5m
  secretStoreRef:
    name: bitwarden-secretstore
    kind: ClusterSecretStore
  target:
    name: api-secrets
    creationPolicy: Owner
    template:
      type: Opaque
      data:
        appsettings.Secrets.json: |
          {
            "JwtOptions": {
              "Secret": "{{ .jwt_secret }}",
              "ConsumerSecret": "{{ .consumer_secret }}"
            },
            "DbOptions": {
              "Host": "postgres-rw.mechanicbuddy-free-tier.svc.cluster.local",
              "Port": 5432,
              "Password": "{{ .db_password }}"
            },
            "SmtpOptions": {
              "Password": "{{ .resend_api_key }}"
            }
          }
  data:
    - secretKey: jwt_secret
      remoteRef:
        key: jwt-secret  # Bitwarden secret name
    - secretKey: consumer_secret
      remoteRef:
        key: consumer-secret
    - secretKey: db_password
      remoteRef:
        key: free-tier-db-password
    - secretKey: resend_api_key
      remoteRef:
        key: resend-api-key
```

---

## Quick Start Commands

### Full Deployment (All Phases)

```bash
cd /home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible

# Set Bitwarden token (get from Bitwarden Secrets Manager)
export BITWARDEN_ACCESS_TOKEN="your-machine-account-token"

# Run complete deployment
ansible-playbook playbooks/rke2-full-deploy.yml --vault-password-file .vault_pass
```

### Individual Phases

```bash
# Phase 1: Infrastructure only
ansible-playbook playbooks/rke2-02-install-addons.yml --vault-password-file .vault_pass

# Phase 2: Secrets only (requires BITWARDEN_ACCESS_TOKEN)
ansible-playbook playbooks/rke2-03-setup-secrets.yml --vault-password-file .vault_pass

# Phase 3: Applications only
ansible-playbook playbooks/rke2-04-deploy-apps.yml --vault-password-file .vault_pass
```

### Verify Deployment

```bash
export KUBECONFIG=/home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible/kubeconfig

# Check all pods
kubectl get pods -A

# Check secrets are synced
kubectl get externalsecrets -A

# Check ingress
kubectl get ingress -A

# Check certificates
kubectl get certificates -A
```

---

## Secrets Rotation

### Rotating a Secret

1. Update secret in Bitwarden Secrets Manager
2. External Secrets Operator automatically syncs (within refresh interval)
3. Pods with `rollme` annotation auto-restart, or manually restart:

```bash
kubectl rollout restart deployment/api -n mechanicbuddy-free-tier
```

### Force Sync

```bash
# Trigger immediate sync
kubectl annotate externalsecret api-secrets -n mechanicbuddy-free-tier force-sync=$(date +%s) --overwrite
```

---

## Backup & Recovery

### Secrets Backup
- All secrets are in Bitwarden (no need to backup from cluster)
- Bitwarden provides audit logs and version history

### Database Backup
- CloudNativePG handles automated backups
- Configure S3/MinIO for backup storage

### Cluster Recovery
1. Re-run `rke2-01-install.yml` for fresh cluster
2. Re-run `rke2-full-deploy.yml` to restore everything
3. Restore database from CloudNativePG backup

---

## Monitoring & Alerts

### Recommended Stack
- **Prometheus** - Metrics collection
- **Grafana** - Dashboards
- **Alertmanager** - Alerts

Install via:
```bash
ansible-playbook playbooks/rke2-05-install-monitoring.yml --vault-password-file .vault_pass
```

---

## Troubleshooting

### Secrets Not Syncing

```bash
# Check ESO logs
kubectl logs -n external-secrets deployment/external-secrets

# Check ExternalSecret status
kubectl describe externalsecret api-secrets -n mechanicbuddy-free-tier

# Check SecretStore connection
kubectl describe clustersecretstore bitwarden-secretstore
```

### Pods Not Starting

```bash
# Check events
kubectl get events -n <namespace> --sort-by='.lastTimestamp'

# Check pod logs
kubectl logs -n <namespace> <pod-name>

# Check secret exists
kubectl get secret <secret-name> -n <namespace> -o yaml
```

### Certificate Issues

```bash
# Check certificate status
kubectl describe certificate -n <namespace>

# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager
```

---

## File Structure

```
infrastructure/ansible/
├── playbooks/
│   ├── rke2-00-reset.yml           # Wipe cluster
│   ├── rke2-01-install.yml         # Install RKE2 cluster
│   ├── rke2-02-install-addons.yml  # Install infrastructure
│   ├── rke2-03-setup-secrets.yml   # Setup Bitwarden + ExternalSecrets
│   ├── rke2-04-deploy-apps.yml     # Deploy MechanicBuddy
│   └── rke2-full-deploy.yml        # All-in-one deployment
│
├── roles/
│   ├── rke2-common/
│   ├── rke2-server/
│   ├── rke2-agent/
│   ├── rke2-ingress-nginx/         # NEW
│   ├── rke2-cert-manager/          # NEW
│   ├── rke2-cloudnative-pg/        # NEW
│   ├── rke2-external-secrets/      # NEW
│   └── rke2-mechanicbuddy/         # NEW
│
├── templates/
│   ├── external-secrets/
│   │   ├── cluster-secret-store.yaml.j2
│   │   ├── api-secrets.yaml.j2
│   │   ├── web-secrets.yaml.j2
│   │   └── ghcr-credentials.yaml.j2
│   └── helm-values/
│       ├── mechanicbuddy-system.yaml.j2
│       └── mechanicbuddy-free-tier.yaml.j2
│
├── kubeconfig                      # Cluster access
├── rke2-token.txt                  # Node join token
├── DEPLOYMENT-PLAN.md              # This file
└── RKE2-SETUP.md                   # Cluster setup docs
```

---

## Next Steps

1. **Create Bitwarden Secrets Manager account** (if not already)
2. **Generate machine account token** for ESO
3. **Add all secrets to Bitwarden** using the structure above
4. **Run deployment playbooks**

Would you like me to create the Ansible playbooks now?
