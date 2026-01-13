# Simplified Approach: Single Control Plane First

## Current Situation

We've been hitting repeated issues with etcd crashing when trying to set up a 5-node HA control plane. The problems stem from:

1. **etcd complexity**: Running a 5-node etcd cluster requires careful coordination
2. **Network/timing issues**: Nodes joining while etcd is still stabilizing causes crashes
3. **Control plane endpoint**: Need a stable endpoint (VIP/load balancer) for true HA

## Recommended Approach

**Start with 1 control plane + 15 workers, then scale up later**

###  Benefits
- ✅ Get the cluster working quickly
- ✅ All 15 workers can join immediately
- ✅ Can deploy workloads and test
- ✅ Can add more control planes later when we have a proper load balancer

### Step 1: Deploy Single Control Plane Cluster

```bash
cd /home/damieno/Development/Freelance/MechanicBuddy/infrastructure/ansible

# 1. Reset everything clean
ansible k8s_cluster -i inventory/hosts.yml -m shell \
  -a "kubeadm reset -f && rm -rf /etc/kubernetes /var/lib/etcd /var/lib/kubelet /etc/cni/net.d ~/.kube /home/ubuntu/.kube" \
  --vault-password-file .vault_pass

# 2. Restart containerd
ansible k8s_cluster -i inventory/hosts.yml -m systemd \
  -a "name=containerd state=restarted" \
  --vault-password-file .vault_pass

# 3. Initialize ONLY control-1
ansible-playbook playbooks/03-install-k8s.yml \
  --vault-password-file .vault_pass \
  --limit k8s-control-1,k8s_workers

# 4. Wait for Calico to be ready (give it 5 minutes)
sleep 300

# 5. Check cluster status
ansible k8s-control-1 -i inventory/hosts.yml -m shell \
  -a "kubectl get nodes" \
  --vault-password-file .vault_pass
```

Expected result: **1 control plane + 15 workers = 16 nodes total**

### Step 2: Add More Control Planes Later (Optional)

Once the single-control-plane cluster is stable and you need HA:

1. **Set up a load balancer**:
   - Option A: HAProxy + Keepalived on 2 small VMs
   - Option B: Use your router/firewall's load balancing
   - Option C: Cloud load balancer if moving to cloud

2. **Update the controlPlaneEndpoint** in the kubeadm config to point to the LB VIP

3. **Join additional control planes**:
   ```bash
   # Get join command from control-1
   ansible k8s-control-1 -i inventory/hosts.yml -m shell \
     -a "kubeadm token create --print-join-command --certificate-key \$(kubeadm init phase upload-certs --upload-certs 2>/dev/null | tail -1)" \
     --vault-password-file .vault_pass

   # Then manually join each control plane with --control-plane flag
   ```

## Why This Approach is Better

1. **Fewer moving parts**: Single etcd instance = no coordination issues
2. **Faster to working state**: Can have workers joined in minutes
3. **Easier to debug**: If something breaks, it's simpler to diagnose
4. **Production-ready**: Many production clusters run with 1-3 control planes, not 5
5. **Can scale later**: Not locked in - can add control planes when you have proper HA infrastructure

## What You Lose (Temporarily)

- ❌ Control plane HA - if control-1 dies, cluster is down
  - But: This is acceptable for dev/test environments
  - Fix: Add HA later with proper load balancer

- ❌ etcd quorum tolerance
  - But: Single etcd is actually very reliable
  - Fix: Can add external etcd cluster later if needed

## Bottom Line

**Get 1 control plane + 15 workers running NOW**, then worry about HA later when you have:
- Proper load balancer infrastructure
- More time to debug etcd issues
- Actual need for control plane HA (many clusters don't!)

The current 3-node cluster you have is actually better than 5 nodes with crashing etcd!
