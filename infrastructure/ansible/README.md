# MechanicBuddy Kubernetes Infrastructure

Ansible playbooks to provision a Kubernetes cluster on Proxmox for the MechanicBuddy SaaS platform.

## Prerequisites

### On your control machine (where you run Ansible):

1. **Ansible 2.15+**
   ```bash
   pip install ansible
   ```

2. **Required Python packages**
   ```bash
   pip install proxmoxer requests
   ```

3. **SSH key pair**
   ```bash
   ssh-keygen -t rsa -b 4096 -f ~/.ssh/id_rsa
   ```

### On Proxmox:

1. **Ubuntu 22.04 Cloud Image Template**

   Download and create a template VM:
   ```bash
   # On Proxmox host
   cd /var/lib/vz/template/iso
   wget https://cloud-images.ubuntu.com/jammy/current/jammy-server-cloudimg-amd64.img

   # Create VM from cloud image
   qm create 9000 --name ubuntu-22.04-cloud --memory 2048 --cores 2 --net0 virtio,bridge=vmbr0
   qm importdisk 9000 jammy-server-cloudimg-amd64.img local-lvm
   qm set 9000 --scsihw virtio-scsi-pci --scsi0 local-lvm:vm-9000-disk-0
   qm set 9000 --boot c --bootdisk scsi0
   qm set 9000 --ide2 local-lvm:cloudinit
   qm set 9000 --serial0 socket --vga serial0
   qm set 9000 --agent enabled=1
   qm template 9000
   ```

2. **API Token**

   Create an API token in Proxmox:
   - Go to Datacenter > Permissions > API Tokens
   - Add a new token for user `root@pam`
   - Token ID: `ansible`
   - Uncheck "Privilege Separation"
   - Save the token secret

## Configuration

1. **Copy and configure vault file**
   ```bash
   cp inventory/group_vars/vault.yml.example inventory/group_vars/vault.yml
   ```

   Edit `vault.yml` with your Proxmox API token:
   ```yaml
   vault_proxmox_api_token_secret: "your-token-secret-here"
   ```

2. **Encrypt vault file**
   ```bash
   ansible-vault encrypt inventory/group_vars/vault.yml
   ```

3. **Configure cluster settings**

   Edit `inventory/group_vars/all.yml` to customize:
   - Proxmox host and node name
   - Network settings (bridge, IPs, gateway)
   - Node specifications (CPU, memory, disk)
   - Kubernetes version
   - Domain settings

## Usage

### Full Deployment

Deploy the complete cluster in one command:
```bash
ansible-playbook site.yml --ask-vault-pass
```

### Step-by-Step Deployment

Run each playbook individually:

```bash
# 1. Check prerequisites
ansible-playbook playbooks/00-prerequisites.yml

# 2. Create VMs on Proxmox
ansible-playbook playbooks/01-create-vms.yml --ask-vault-pass

# 3. Prepare nodes (install containerd, kubeadm, etc.)
ansible-playbook playbooks/02-prepare-nodes.yml

# 4. Initialize Kubernetes cluster
ansible-playbook playbooks/03-install-k8s.yml

# 5. Install addons (ingress, cert-manager, CloudNativePG)
ansible-playbook playbooks/04-install-addons.yml
```

### Destroy Cluster

**WARNING: This permanently deletes all VMs and data!**

```bash
ansible-playbook playbooks/99-destroy-cluster.yml --ask-vault-pass
```

## Cluster Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      PROXMOX CLUSTER                        │
├─────────────────────────────────────────────────────────────┤
│  VM: k8s-control-1          VM: k8s-worker-1               │
│  ├─ Ubuntu 22.04 LTS        ├─ Ubuntu 22.04 LTS            │
│  ├─ 4 vCPU, 8GB RAM         ├─ 4 vCPU, 16GB RAM            │
│  ├─ 50GB disk               ├─ 100GB disk                  │
│  └─ Control plane           └─ Worker node                 │
│                                                             │
│  VM: k8s-worker-2                                          │
│  ├─ Ubuntu 22.04 LTS                                       │
│  ├─ 4 vCPU, 16GB RAM                                       │
│  ├─ 100GB disk                                             │
│  └─ Worker node                                            │
└─────────────────────────────────────────────────────────────┘
```

## Installed Components

| Component | Version | Purpose |
|-----------|---------|---------|
| Kubernetes | 1.29 | Container orchestration |
| containerd | 1.7.x | Container runtime |
| Calico | 3.27.0 | CNI networking |
| ingress-nginx | 1.9.6 | Ingress controller |
| cert-manager | 1.14.3 | TLS certificate management |
| CloudNativePG | 1.22.1 | PostgreSQL operator |
| local-path-provisioner | 0.0.26 | Storage provisioner |

## Post-Installation

### Access the cluster

```bash
export KUBECONFIG=./kubeconfig
kubectl get nodes
```

### Configure DNS

Point your domain to the worker node IPs:
```
*.mechanicbuddy.app -> 192.168.1.101
```

Or configure round-robin DNS:
```
*.mechanicbuddy.app -> 192.168.1.101
*.mechanicbuddy.app -> 192.168.1.102
```

### Test ingress and TLS

```bash
# Create a test deployment
kubectl create deployment hello --image=nginxdemos/hello
kubectl expose deployment hello --port=80

# Create ingress with TLS
cat <<EOF | kubectl apply -f -
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: hello
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - test.mechanicbuddy.app
    secretName: hello-tls
  rules:
  - host: test.mechanicbuddy.app
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: hello
            port:
              number: 80
EOF
```

## Troubleshooting

### VM creation fails

- Verify Proxmox API token permissions
- Check template VM exists (ID 9000)
- Ensure sufficient resources on Proxmox host

### Nodes not joining cluster

- Check network connectivity between nodes
- Verify firewall allows ports 6443, 10250, 10251, 10252
- Check join token hasn't expired

### Pods stuck in Pending

- Verify CNI is installed: `kubectl get pods -n calico-system`
- Check node status: `kubectl describe node <node-name>`

### cert-manager not issuing certificates

- Verify DNS is pointing to ingress
- Check ClusterIssuer: `kubectl describe clusterissuer letsencrypt-prod`
- Check certificate: `kubectl describe certificate <cert-name>`

## Directory Structure

```
infrastructure/ansible/
├── ansible.cfg                    # Ansible configuration
├── site.yml                       # Main playbook (runs all)
├── inventory/
│   ├── hosts.yml                  # Host inventory
│   └── group_vars/
│       ├── all.yml                # Configuration variables
│       └── vault.yml              # Encrypted secrets
├── playbooks/
│   ├── 00-prerequisites.yml       # Check prerequisites
│   ├── 01-create-vms.yml          # Create Proxmox VMs
│   ├── 02-prepare-nodes.yml       # Install K8s prerequisites
│   ├── 03-install-k8s.yml         # Initialize cluster
│   ├── 04-install-addons.yml      # Install addons
│   └── 99-destroy-cluster.yml     # Destroy cluster
├── roles/
│   ├── proxmox-vm/                # VM creation role
│   ├── k8s-prerequisites/         # containerd, kubeadm, etc.
│   ├── k8s-control-plane/         # kubeadm init
│   ├── k8s-worker/                # kubeadm join
│   ├── k8s-cni/                   # Calico CNI
│   ├── k8s-ingress-nginx/         # Ingress controller
│   ├── k8s-cert-manager/          # TLS certificates
│   ├── k8s-cloudnative-pg/        # PostgreSQL operator
│   └── k8s-storage/               # Storage provisioner
└── templates/
    ├── cloud-init-user-data.yml.j2
    └── kubeadm-config.yml.j2
```
