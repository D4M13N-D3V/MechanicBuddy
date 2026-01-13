# RKE2 Kubernetes Cluster Setup

This guide explains how to deploy a 20-node Kubernetes cluster using RKE2 (Rancher Kubernetes Engine 2) with Ansible automation.

## Overview

RKE2 is Rancher's next-generation Kubernetes distribution that is fully conformant, highly secure, and production-ready. It includes:

- **Built-in security**: CIS hardening, SELinux support
- **Simplified operations**: Single binary installation, automatic certificate rotation
- **Embedded components**: etcd, CoreDNS, metrics-server, local path provisioner
- **HA ready**: Native support for multi-master clusters

## Cluster Architecture

- **5 Control Plane Nodes**: HA etcd cluster with 3-node quorum minimum (5 provides extra redundancy)
- **15 Worker Nodes**: Workload execution nodes
- **Total**: 20 nodes

## Prerequisites

1. **VMs created on Proxmox** with proper disk space (run VM creation playbooks first)
2. **SSH access** configured to all nodes
3. **Ansible vault** password file at `.vault_pass`
4. **Python environment** with required packages (created by `00-prerequisites.yml`)

## Quick Start

### Step 1: Reset Existing Kubernetes Installation (if any)

If you have an existing kubeadm or RKE2 installation, completely wipe it first:

```bash
cd /home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible
ansible-playbook playbooks/rke2-00-reset.yml --vault-password-file .vault_pass
```

This playbook will:
- Stop all RKE2 and kubeadm services
- Remove all Kubernetes directories and data
- Clean up network configurations (iptables, CNI)
- Remove RKE2 binaries
- Prepare nodes for fresh installation

**⚠️ WARNING**: This is destructive and will delete all Kubernetes data including etcd!

### Step 2: Install RKE2 Cluster

```bash
ansible-playbook playbooks/rke2-01-install.yml --vault-password-file .vault_pass
```

This playbook will:
1. Install RKE2 on the first control plane (bootstrap node)
2. Generate and distribute the node token
3. Join additional 4 control planes one at a time
4. Join all 15 worker nodes (2 at a time for stability)
5. Configure kubectl access
6. Verify all nodes are ready
7. Save kubeconfig to `infrastructure/ansible/kubeconfig`

**Duration**: Expect 15-20 minutes for full cluster initialization.

### Step 3: Verify Cluster

```bash
export KUBECONFIG=/home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible/kubeconfig
kubectl get nodes -o wide
```

You should see all 20 nodes in `Ready` state:
- 5 control plane nodes with `control-plane` role
- 15 worker nodes with `<none>` or `worker` role

Check cluster health:
```bash
kubectl get pods -A
kubectl cluster-info
```

## What Gets Installed

RKE2 comes with these components pre-installed:

### Core Components
- **etcd**: Distributed key-value store (on control planes)
- **kube-apiserver**: Kubernetes API (on control planes)
- **kube-controller-manager**: Core controllers (on control planes)
- **kube-scheduler**: Pod scheduler (on control planes)
- **kubelet**: Node agent (all nodes)
- **containerd**: Container runtime (all nodes)

### Add-ons (Automatically Installed)
- **CoreDNS**: Cluster DNS service
- **Metrics Server**: Resource usage metrics for `kubectl top`
- **Local Path Provisioner**: Default storage class for PersistentVolumes

### Disabled by Default
- **Traefik Ingress**: Disabled in our config (you can use NGINX Ingress or others)

## Configuration Details

### Control Plane Configuration

See [roles/rke2-server/templates/config.yaml.j2](roles/rke2-server/templates/config.yaml.j2):

- **TLS SANs**: All control plane IPs + VIP (192.168.1.100)
- **Node taints**: Control planes tainted to prevent workload scheduling
- **Cluster CIDR**: 10.42.0.0/16 (pod network)
- **Service CIDR**: 10.43.0.0/16 (service network)
- **Disabled components**: Traefik ingress (install your own)

### Worker Node Configuration

See [roles/rke2-agent/templates/config.yaml.j2](roles/rke2-agent/templates/config.yaml.j2):

- **Server**: Connects to 192.168.1.100:9345 (RKE2 supervisor port)
- **Node labels**: Custom labels can be added per node

## File Locations

### On Cluster Nodes

- **Config**: `/etc/rancher/rke2/config.yaml`
- **Kubeconfig**: `/etc/rancher/rke2/rke2.yaml`
- **Data directory**: `/var/lib/rancher/rke2`
- **Binaries**: `/var/lib/rancher/rke2/bin/` (kubectl, crictl, etc.)
- **Service**: `rke2-server.service` or `rke2-agent.service`

### On Ansible Controller

- **Kubeconfig**: `infrastructure/ansible/kubeconfig`
- **Node token**: `infrastructure/ansible/rke2-token.txt`
- **Playbooks**: `infrastructure/ansible/playbooks/rke2-*.yml`
- **Roles**: `infrastructure/ansible/roles/rke2-*/`

## Troubleshooting

### Check RKE2 Service Status

**On control planes:**
```bash
sudo systemctl status rke2-server
sudo journalctl -u rke2-server -f
```

**On workers:**
```bash
sudo systemctl status rke2-agent
sudo journalctl -u rke2-agent -f
```

### Check Node Readiness

```bash
kubectl get nodes
kubectl describe node <node-name>
```

### Common Issues

#### Node Not Joining
- **Check token**: Ensure `rke2-token.txt` exists and is readable
- **Check connectivity**: Verify node can reach 192.168.1.100:9345
- **Check logs**: `journalctl -u rke2-agent -n 100`

#### Control Plane Not Starting
- **Check etcd**: `sudo /var/lib/rancher/rke2/bin/crictl ps | grep etcd`
- **Check API server**: `curl -k https://localhost:6443/healthz`
- **Check disk space**: `df -h`

#### Pods Not Starting
- **Check containerd**: `sudo systemctl status containerd`
- **Check images**: `sudo /var/lib/rancher/rke2/bin/crictl images`
- **Check CNI**: `kubectl get pods -n kube-system | grep coredns`

### Reset Single Node

If a single node is misbehaving, reset just that node:

```bash
# On the problem node
sudo /usr/local/bin/rke2-uninstall.sh  # or /usr/bin/rke2-uninstall.sh
sudo rm -rf /etc/rancher /var/lib/rancher /var/lib/kubelet

# Then re-run the install playbook
ansible-playbook playbooks/rke2-01-install.yml --vault-password-file .vault_pass --limit <node-name>
```

## RKE2 vs Kubeadm

Why we switched from kubeadm to RKE2:

| Feature | Kubeadm | RKE2 |
|---------|---------|------|
| **Installation** | Multi-step, complex | Single binary |
| **Upgrades** | Manual, risky | Built-in upgrade controller |
| **Security** | Manual hardening | CIS hardened by default |
| **HA Setup** | Complex (etcd, LB) | Built-in HA support |
| **Components** | Separate installation | Bundled together |
| **Stability** | Can be fragile | Production-hardened |

## Next Steps

### Install Storage Provider

RKE2 includes local-path provisioner, but for production you may want:

- **Longhorn**: Distributed block storage
- **Rook/Ceph**: Cloud-native storage orchestration
- **NFS CSI**: NFS-backed storage

### Install Ingress Controller

```bash
# Option 1: NGINX Ingress
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.10.0/deploy/static/provider/baremetal/deploy.yaml

# Option 2: Enable Traefik (disabled by default)
# Edit rke2-server config to remove disable: ["traefik"] and restart
```

### Install Monitoring

```bash
# Prometheus + Grafana stack
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install kube-prometheus-stack prometheus-community/kube-prometheus-stack
```

### Deploy Your Application

Your MechanicBuddy application can now be deployed to this cluster!

## Maintenance

### Upgrade RKE2

```bash
# On each node (control planes first, then workers)
curl -sfL https://get.rke2.io | INSTALL_RKE2_VERSION=vX.XX.X sh -
sudo systemctl restart rke2-server  # or rke2-agent
```

### Backup etcd

```bash
# On any control plane
sudo /var/lib/rancher/rke2/bin/etcdctl snapshot save backup.db \
  --endpoints=https://127.0.0.1:2379 \
  --cacert=/var/lib/rancher/rke2/server/tls/etcd/server-ca.crt \
  --cert=/var/lib/rancher/rke2/server/tls/etcd/server-client.crt \
  --key=/var/lib/rancher/rke2/server/tls/etcd/server-client.key
```

## Support

- **RKE2 Docs**: https://docs.rke2.io
- **Rancher Slack**: https://slack.rancher.io
- **GitHub Issues**: https://github.com/rancher/rke2/issues

## Files Created

This RKE2 setup created the following Ansible files:

- `playbooks/rke2-00-reset.yml` - Complete cluster wipe
- `playbooks/rke2-01-install.yml` - Full cluster installation
- `roles/rke2-common/tasks/main.yml` - Common prerequisites
- `roles/rke2-server/tasks/main.yml` - Control plane installation
- `roles/rke2-server/templates/config.yaml.j2` - Server config
- `roles/rke2-agent/tasks/main.yml` - Worker installation
- `roles/rke2-agent/templates/config.yaml.j2` - Agent config

Inventory at `inventory/hosts.yml` works as-is with these playbooks.
