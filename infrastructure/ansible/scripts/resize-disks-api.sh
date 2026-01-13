#!/bin/bash

# Proxmox API configuration
PROXMOX_HOST="proxmox.d4m13n.dev"
PROXMOX_PORT="443"
API_USER="root@pam"
TOKEN_ID="ansible"

# Prompt for token secret
echo "Enter Proxmox API token secret:"
read -s TOKEN_SECRET

AUTH_HEADER="Authorization: PVEAPIToken=${API_USER}!${TOKEN_ID}=${TOKEN_SECRET}"

echo ""
echo "Resizing VM disks..."
echo ""

# Control planes (50G)
declare -A CONTROL_PLANES=(
    ["200"]="medusa:k8s-control-1:50G"
    ["210"]="iris:k8s-control-2:50G"
    ["211"]="medusa:k8s-control-3:50G"
    ["212"]="iris:k8s-control-4:50G"
    ["213"]="medusa:k8s-control-5:50G"
)

# Workers (100G)
declare -A WORKERS=(
    ["201"]="iris:k8s-worker-1:100G"
    ["202"]="medusa:k8s-worker-2:100G"
    ["203"]="iris:k8s-worker-3:100G"
    ["204"]="medusa:k8s-worker-4:100G"
    ["205"]="iris:k8s-worker-5:100G"
    ["206"]="medusa:k8s-worker-6:100G"
    ["207"]="iris:k8s-worker-7:100G"
    ["208"]="medusa:k8s-worker-8:100G"
    ["209"]="iris:k8s-worker-9:100G"
    ["214"]="medusa:k8s-worker-10:100G"
    ["215"]="iris:k8s-worker-11:100G"
    ["216"]="medusa:k8s-worker-12:100G"
    ["218"]="iris:k8s-worker-13:100G"
    ["219"]="medusa:k8s-worker-14:100G"
    ["220"]="iris:k8s-worker-15:100G"
)

# Resize control planes
for vmid in "${!CONTROL_PLANES[@]}"; do
    IFS=':' read -r node name size <<< "${CONTROL_PLANES[$vmid]}"

    echo "Resizing $name (VMID: $vmid) on node $node to $size..."
    response=$(curl -sk -X PUT \
        "https://${PROXMOX_HOST}:${PROXMOX_PORT}/api2/json/nodes/${node}/qemu/${vmid}/resize" \
        -H "${AUTH_HEADER}" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        --data-urlencode "disk=scsi0" \
        --data-urlencode "size=${size}")

    if echo "$response" | grep -q '"data"'; then
        echo "✓ Successfully resized $name"
    else
        echo "✗ Failed or already at size: $name"
        echo "  Response: $response"
    fi
    echo ""
done

# Resize workers
for vmid in "${!WORKERS[@]}"; do
    IFS=':' read -r node name size <<< "${WORKERS[$vmid]}"

    echo "Resizing $name (VMID: $vmid) on node $node to $size..."
    response=$(curl -sk -X PUT \
        "https://${PROXMOX_HOST}:${PROXMOX_PORT}/api2/json/nodes/${node}/qemu/${vmid}/resize" \
        -H "${AUTH_HEADER}" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        --data-urlencode "disk=scsi0" \
        --data-urlencode "size=${size}")

    if echo "$response" | grep -q '"data"'; then
        echo "✓ Successfully resized $name"
    else
        echo "✗ Failed or already at size: $name"
        echo "  Response: $response"
    fi
    echo ""
done

echo "Resize complete! Now run the filesystem extend playbook."
