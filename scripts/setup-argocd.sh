#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== MechanicBuddy ArgoCD Setup ===${NC}"

# Check prerequisites
command -v kubectl >/dev/null 2>&1 || { echo -e "${RED}kubectl is required but not installed.${NC}" >&2; exit 1; }
command -v argocd >/dev/null 2>&1 || { echo -e "${YELLOW}argocd CLI not found. Some operations will be skipped.${NC}"; }

# Configuration
ARGOCD_NAMESPACE="argocd"
PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
REPO_URL="${REPO_URL:-https://github.com/YOUR_ORG/mechanicbuddy.git}"

echo -e "\n${GREEN}1. Installing ArgoCD...${NC}"
if kubectl get namespace "$ARGOCD_NAMESPACE" >/dev/null 2>&1; then
    echo "ArgoCD namespace already exists"
else
    kubectl create namespace "$ARGOCD_NAMESPACE"
fi

# Install ArgoCD
kubectl apply -n "$ARGOCD_NAMESPACE" -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

echo -e "\n${GREEN}2. Waiting for ArgoCD to be ready...${NC}"
kubectl wait --for=condition=available --timeout=300s deployment/argocd-server -n "$ARGOCD_NAMESPACE"

echo -e "\n${GREEN}3. Getting ArgoCD admin password...${NC}"
ARGOCD_PASSWORD=$(kubectl -n "$ARGOCD_NAMESPACE" get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d)
echo -e "ArgoCD Admin Password: ${YELLOW}$ARGOCD_PASSWORD${NC}"
echo "Username: admin"

echo -e "\n${GREEN}4. Creating ArgoCD Project...${NC}"
kubectl apply -f "$PROJECT_DIR/infrastructure/argocd/projects/mechanicbuddy.yaml"

echo -e "\n${GREEN}5. To add the repository (run manually):${NC}"
echo -e "${YELLOW}argocd login <argocd-server-url>${NC}"
echo -e "${YELLOW}argocd repo add $REPO_URL --username git --password \$GITHUB_TOKEN${NC}"

echo -e "\n${GREEN}6. Deploy staging environment (run manually):${NC}"
echo -e "${YELLOW}kubectl apply -f $PROJECT_DIR/infrastructure/argocd/apps/system-staging.yaml${NC}"

echo -e "\n${GREEN}7. Deploy production environment (run manually):${NC}"
echo -e "${YELLOW}kubectl apply -f $PROJECT_DIR/infrastructure/argocd/apps/system-production.yaml${NC}"

echo -e "\n${GREEN}8. Setting up Ingress for remote access...${NC}"
if [ -f "$PROJECT_DIR/infrastructure/argocd/argocd-ingress.yaml" ]; then
    kubectl apply -f "$PROJECT_DIR/infrastructure/argocd/argocd-ingress.yaml"
    echo "ArgoCD Ingress created - accessible at https://argocd.mechanicbuddy.app"
else
    echo -e "${YELLOW}Ingress file not found, skipping...${NC}"
fi

echo -e "\n${GREEN}=== ArgoCD Setup Complete ===${NC}"
echo -e "\nAccess Options:"
echo ""
echo -e "${YELLOW}Option 1: Via Ingress (if DNS is configured)${NC}"
echo "  URL: https://argocd.mechanicbuddy.app"
echo "  Make sure argocd.mechanicbuddy.app points to your cluster's ingress IP"
echo ""
echo -e "${YELLOW}Option 2: Via Port Forward (from a machine with kubectl access)${NC}"
echo "  kubectl port-forward svc/argocd-server -n argocd 8080:443"
echo "  Then visit: https://localhost:8080"
echo ""
echo -e "${YELLOW}Option 3: Via SSH Tunnel (from remote machine)${NC}"
echo "  ssh -L 8080:localhost:8080 user@control-plane-node"
echo "  Then on the control plane: kubectl port-forward svc/argocd-server -n argocd 8080:443"
echo "  Then visit locally: https://localhost:8080"
echo ""
echo -e "Credentials:"
echo "  Username: admin"
echo -e "  Password: ${YELLOW}$ARGOCD_PASSWORD${NC}"
echo ""
echo -e "Next steps:"
echo "1. Login to ArgoCD UI"
echo "2. Add your GitHub repository"
echo "3. Configure secrets in overlays (use SealedSecrets or External Secrets)"
echo "4. Deploy the staging application"
