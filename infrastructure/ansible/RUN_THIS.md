# MechanicBuddy Deployment - Quick Start

## Current Status

- **RKE2 Cluster**: 5 control planes + 15 workers (all running)
- **Next Step**: Deploy infrastructure addons and MechanicBuddy application

## Step 1: Update Vault Secrets

Edit your encrypted vault to add new variables:

```bash
cd /home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible
ansible-vault edit inventory/group_vars/all/vault.yml
```

Add these (see `vault.yml.example` for full template):

```yaml
# Bitwarden API keys (from https://bitwarden.d4m13n.dev/#/settings/security/security-keys)
bitwarden_client_id: "user.xxx..."
bitwarden_client_secret: "xxx..."
bitwarden_master_password: "your-password"

# GHCR token (from https://github.com/settings/tokens - needs read:packages)
ghcr_username: "d4m13n-d3v"
ghcr_token: "ghp_xxx..."

# Resend API key (from https://resend.com/api-keys)
resend_api_key: "re_xxx..."
```

## Step 2: Deploy Everything

### Option A: Full Deploy (Recommended)

```bash
ansible-playbook playbooks/rke2-full-deploy.yml --vault-password-file .vault_pass
```

### Option B: Step by Step

```bash
# 1. Install infrastructure (ingress, cert-manager, cloudnative-pg, bitwarden-operator)
ansible-playbook playbooks/rke2-02-install-addons.yml --vault-password-file .vault_pass

# 2. Setup Bitwarden secrets integration
ansible-playbook playbooks/rke2-03-setup-secrets.yml --vault-password-file .vault_pass

# 3. Deploy MechanicBuddy application
ansible-playbook playbooks/rke2-04-deploy-apps.yml --vault-password-file .vault_pass
```

## Step 3: Verify Deployment

```bash
export KUBECONFIG=/home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible/kubeconfig

# Check all pods
kubectl get pods -A

# Check MechanicBuddy specifically
kubectl get pods -n mechanicbuddy-free-tier
kubectl get pods -n mechanicbuddy-system

# Check ingress
kubectl get ingress -A

# Check certificates
kubectl get certificates -A

# Check PostgreSQL clusters
kubectl get clusters.postgresql.cnpg.io -A
```

## Step 4: Configure External Proxy

Your nginx proxy (136.56.217.220) should forward to the cluster NodePorts:

```nginx
upstream k8s_http {
    server 192.168.1.101:30080;
    server 192.168.1.102:30080;
    server 192.168.1.103:30080;
}

upstream k8s_https {
    server 192.168.1.101:30443;
    server 192.168.1.102:30443;
    server 192.168.1.103:30443;
}

server {
    listen 80;
    server_name *.mechanicbuddy.app mechanicbuddy.app;
    location / {
        proxy_pass http://k8s_http;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 443 ssl;
    server_name *.mechanicbuddy.app mechanicbuddy.app;
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    location / {
        proxy_pass https://k8s_https;
        proxy_ssl_verify off;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto https;
    }
}
```

## Troubleshooting

### Pods not starting?
```bash
kubectl get events -n mechanicbuddy-free-tier --sort-by='.lastTimestamp'
kubectl logs -n mechanicbuddy-free-tier -l app=api
kubectl describe pod -n mechanicbuddy-free-tier -l app=api
```

### Image pull errors?
```bash
kubectl get secret ghcr-credentials -n mechanicbuddy-free-tier -o yaml
```

### Database issues?
```bash
kubectl get clusters.postgresql.cnpg.io -A
kubectl logs -n mechanicbuddy-system -l cnpg.io/cluster=mechanicbuddy-management-db
```

### Certificate issues?
```bash
kubectl describe certificate -n mechanicbuddy-free-tier
kubectl logs -n cert-manager -l app=cert-manager
```

## Architecture

```
Internet -> nginx (136.56.217.220) -> NodePort (30080/30443) -> Ingress Controller -> Services
                                                                       |
                                                          +------------+------------+
                                                          |                         |
                                              mechanicbuddy-free-tier    mechanicbuddy-system
                                                  - API (2 replicas)        - Management DB
                                                  - Web (2 replicas)        - (future: Portal)
                                                  - PostgreSQL
```

## Key Files

| File | Purpose |
|------|---------|
| `playbooks/rke2-00-reset.yml` | Wipe cluster (if needed) |
| `playbooks/rke2-01-install.yml` | Install RKE2 cluster |
| `playbooks/rke2-02-install-addons.yml` | Install ingress, cert-manager, etc. |
| `playbooks/rke2-03-setup-secrets.yml` | Setup Bitwarden integration |
| `playbooks/rke2-04-deploy-apps.yml` | Deploy MechanicBuddy |
| `playbooks/rke2-full-deploy.yml` | All-in-one deployment |
| `kubeconfig` | kubectl access to cluster |
| `generated-secrets.yml` | Auto-generated secrets (after deploy) |

## Secrets Management

Secrets are managed via **Bitwarden Secret Operator**:
- Operator syncs secrets from your Bitwarden vault to Kubernetes
- Create items in Bitwarden with custom fields
- Secrets auto-sync every 5 minutes

Or use **Ansible Vault** directly:
- All sensitive values in `inventory/group_vars/all/vault.yml`
- Encrypted with `.vault_pass` file
- Playbooks read secrets and create Kubernetes secrets
