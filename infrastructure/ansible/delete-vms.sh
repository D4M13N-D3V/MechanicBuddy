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

# VMs to delete: control-2 to control-5, worker-10 to worker-15
declare -A VMS_TO_DELETE=(
    ["210"]="iris:k8s-control-2"
    ["211"]="medusa:k8s-control-3"
    ["212"]="iris:k8s-control-4"
    ["213"]="medusa:k8s-control-5"
    ["214"]="medusa:k8s-worker-10"
    ["215"]="iris:k8s-worker-11"
    ["216"]="medusa:k8s-worker-12"
    ["218"]="iris:k8s-worker-13"
    ["219"]="medusa:k8s-worker-14"
    ["220"]="iris:k8s-worker-15"
)

echo ""
echo "Deleting VMs..."
echo ""

for vmid in "${!VMS_TO_DELETE[@]}"; do
    IFS=':' read -r node name <<< "${VMS_TO_DELETE[$vmid]}"

    echo "Stopping VM $name (VMID: $vmid) on node $node..."
    curl -sk -X POST \
        "https://${PROXMOX_HOST}:${PROXMOX_PORT}/api2/json/nodes/${node}/qemu/${vmid}/status/stop" \
        -H "${AUTH_HEADER}" \
        > /dev/null 2>&1

    sleep 2

    echo "Deleting VM $name (VMID: $vmid) on node $node..."
    response=$(curl -sk -X DELETE \
        "https://${PROXMOX_HOST}:${PROXMOX_PORT}/api2/json/nodes/${node}/qemu/${vmid}" \
        -H "${AUTH_HEADER}")

    if echo "$response" | grep -q '"data"'; then
        echo "✓ Successfully deleted $name"
    else
        echo "✗ Failed to delete $name: $response"
    fi

    echo ""
done

echo "Deletion complete!"
