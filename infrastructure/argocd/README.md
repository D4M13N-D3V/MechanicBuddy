# ArgoCD GitOps Deployment

This directory contains ArgoCD application definitions for deploying MechanicBuddy SaaS platform.

## Structure

```
argocd/
├── projects/
│   └── mechanicbuddy.yaml      # ArgoCD Project definition
├── apps/
│   ├── system-staging.yaml     # Management system (staging)
│   ├── system-production.yaml  # Management system (production)
│   └── app-of-apps.yaml        # Root application (optional)
└── README.md
```

## Prerequisites

1. ArgoCD installed on your cluster
2. ArgoCD CLI configured
3. Repository added to ArgoCD

## Setup

### 1. Install ArgoCD (if not already installed)

```bash
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

### 2. Add this repository to ArgoCD

```bash
argocd repo add https://github.com/YOUR_ORG/mechanicbuddy.git \
  --username git \
  --password $GITHUB_TOKEN
```

### 3. Create the ArgoCD Project

```bash
kubectl apply -f infrastructure/argocd/projects/mechanicbuddy.yaml
```

### 4. Deploy Applications

```bash
# Deploy staging
kubectl apply -f infrastructure/argocd/apps/system-staging.yaml

# Deploy production (when ready)
kubectl apply -f infrastructure/argocd/apps/system-production.yaml
```

## Image Updates

The CI/CD pipeline updates image tags in the overlay kustomization files. ArgoCD detects these changes and syncs automatically.

## Secrets Management

Secrets are managed via Sealed Secrets or External Secrets Operator. See `infrastructure/k8s/overlays/*/secrets/` for templates.
