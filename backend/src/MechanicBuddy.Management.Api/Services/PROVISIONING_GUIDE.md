# Tenant Provisioning Guide

Quick reference guide for provisioning MechanicBuddy tenants using Kubernetes and Helm.

## Quick Start

### 1. Prerequisites

```bash
# Verify Kubernetes cluster access
kubectl cluster-info

# Verify Helm installation
helm version

# Check RBAC permissions
kubectl auth can-i create namespaces
kubectl auth can-i create pods --all-namespaces
```

### 2. Configuration

Edit `appsettings.Provisioning.json`:

```json
{
  "Provisioning": {
    "HelmChartPath": "/app/infrastructure/helm/charts/mechanicbuddy-tenant",
    "BaseDomain": "mechanicbuddy.app",
    "StorageClass": "local-path"
  }
}
```

### 3. Start Management API

```bash
cd backend/src/MechanicBuddy.Management.Api
dotnet run
```

## API Endpoints

### POST /api/tenants - Provision Tenant

Creates a new tenant with full infrastructure.

**Request:**
```json
{
  "companyName": "Acme Auto Repair",
  "tenantId": "acme-auto",  // Optional, auto-generated if omitted
  "ownerEmail": "owner@acmeauto.com",
  "ownerFirstName": "John",
  "ownerLastName": "Doe",
  "subscriptionTier": "professional",
  "customDomain": "shop.acmeauto.com",  // Optional
  "stripeCustomerId": "cus_123456",  // Optional
  "stripeSubscriptionId": "sub_123456",  // Optional
  "populateSampleData": false,
  "additionalEnvVars": {  // Optional
    "FEATURE_FLAGS": "advanced-reporting"
  },
  "resourceOverrides": {  // Optional, for custom deployments
    "postgresInstances": 3,
    "apiReplicas": 5
  }
}
```

**Response (Success):**
```json
{
  "success": true,
  "tenantId": "acme-auto",
  "tenantUrl": "https://acme-auto.mechanicbuddy.app",
  "apiUrl": "https://acme-auto.mechanicbuddy.app/api",
  "namespace": "tenant-acme-auto",
  "adminUsername": "admin",
  "adminPassword": "ChangeMeOnFirstLogin!",
  "helmRelease": "tenant-acme-auto",
  "subscriptionTier": "professional",
  "stripeCustomerId": "cus_123456",
  "provisionedAt": "2024-01-15T10:30:00Z",
  "provisioningDuration": "00:02:45",
  "resources": {
    "postgresInstances": 1,
    "postgresStorage": "50Gi",
    "apiReplicas": 2,
    "webReplicas": 2,
    "mechanicLimit": 20,
    "backupEnabled": true
  },
  "provisioningLog": [
    {
      "timestamp": "2024-01-15T10:30:00Z",
      "level": "Info",
      "step": "ValidateRequest",
      "message": "Validating provisioning request"
    },
    {
      "timestamp": "2024-01-15T10:30:01Z",
      "level": "Info",
      "step": "GenerateTenantId",
      "message": "Generated tenant ID: acme-auto"
    },
    {
      "timestamp": "2024-01-15T10:30:05Z",
      "level": "Info",
      "step": "DeployHelm",
      "message": "Deploying Helm chart to namespace tenant-acme-auto"
    },
    {
      "timestamp": "2024-01-15T10:32:30Z",
      "level": "Info",
      "step": "Complete",
      "message": "Tenant provisioned successfully in 145.2s"
    }
  ]
}
```

**Response (Failure):**
```json
{
  "success": false,
  "errorMessage": "Tenant with ID 'acme-auto' already exists",
  "provisioningLog": [...]
}
```

### GET /api/tenants/{tenantId}/status - Get Tenant Status

Retrieves current deployment health and status.

**Response:**
```json
{
  "tenantId": "acme-auto",
  "namespace": "tenant-acme-auto",
  "isHealthy": true,
  "status": "Healthy",
  "tenantUrl": "https://acme-auto.mechanicbuddy.app",
  "lastChecked": "2024-01-15T10:35:00Z",
  "database": {
    "isReady": true,
    "status": "Ready",
    "instances": 1,
    "readyInstances": 1
  },
  "pods": [
    {
      "name": "acme-auto-postgres-1",
      "phase": "Running",
      "ready": true,
      "readyContainers": 1,
      "totalContainers": 1,
      "containerStatuses": ["postgres: Ready"],
      "startTime": "2024-01-15T10:30:30Z"
    },
    {
      "name": "acme-auto-api-7d8c9f-abc",
      "phase": "Running",
      "ready": true,
      "readyContainers": 1,
      "totalContainers": 1,
      "containerStatuses": ["api: Ready"],
      "startTime": "2024-01-15T10:31:00Z"
    },
    {
      "name": "acme-auto-web-5f6g7h-def",
      "phase": "Running",
      "ready": true,
      "readyContainers": 1,
      "totalContainers": 1,
      "containerStatuses": ["web: Ready"],
      "startTime": "2024-01-15T10:31:15Z"
    }
  ]
}
```

### PUT /api/tenants/{tenantId} - Update Tenant

Updates an existing tenant deployment (scaling, tier change, etc.).

**Request:**
```json
{
  "companyName": "Acme Auto Repair",
  "ownerEmail": "owner@acmeauto.com",
  "ownerFirstName": "John",
  "ownerLastName": "Doe",
  "subscriptionTier": "enterprise",  // Upgrade from professional
  "customDomain": "shop.acmeauto.com"
}
```

**Response:** Same as provisioning response

### DELETE /api/tenants/{tenantId} - Deprovision Tenant

Removes all tenant resources and cleans up namespace.

**Response:**
```json
{
  "message": "Tenant deprovisioned successfully",
  "tenantId": "acme-auto"
}
```

### POST /api/tenants/validate - Validate Request

Validates a provisioning request without actually provisioning.

**Request:** Same as provision request

**Response:**
```json
{
  "isValid": true,
  "errors": [],
  "warnings": []
}
```

**Response (Invalid):**
```json
{
  "isValid": false,
  "errors": [
    "Tenant with ID 'acme-auto' already exists",
    "Invalid subscription tier: premium"
  ],
  "warnings": [
    "Custom domain requires DNS configuration"
  ]
}
```

### GET /api/tenants/generate-id?companyName=Acme Auto Repair

Generates a tenant ID from a company name.

**Response:**
```json
{
  "companyName": "Acme Auto Repair",
  "tenantId": "acme-auto-a1b2c3"
}
```

## Subscription Tiers

### Demo (Trial)
```json
{
  "subscriptionTier": "demo"
}
```
- **Duration:** 7 days
- **Storage:** 5Gi
- **Mechanics:** 2
- **Instances:** 1 PostgreSQL, 1 API, 1 Web
- **RAM:** 128-256Mi per service
- **Backup:** No

### Free
```json
{
  "subscriptionTier": "free"
}
```
- **Duration:** Unlimited
- **Storage:** 10Gi
- **Mechanics:** 5
- **Instances:** 1 PostgreSQL, 1 API, 1 Web
- **RAM:** 256-512Mi per service
- **Backup:** No

### Professional
```json
{
  "subscriptionTier": "professional"
}
```
- **Duration:** Unlimited
- **Storage:** 50Gi
- **Mechanics:** 20
- **Instances:** 1 PostgreSQL, 2 API, 2 Web
- **RAM:** 512Mi-1Gi per service
- **Backup:** Yes (7 days retention)

### Enterprise
```json
{
  "subscriptionTier": "enterprise"
}
```
- **Duration:** Unlimited
- **Storage:** 200Gi
- **Mechanics:** Unlimited
- **Instances:** 3 PostgreSQL (HA), 3 API, 3 Web
- **RAM:** 1-2Gi per service
- **Backup:** Yes (30 days retention)

## Provisioning Flow

```
1. Receive Request → Validate
   ├─ Check cluster accessibility
   ├─ Check Helm availability
   ├─ Validate tenant ID uniqueness
   └─ Validate tier configuration

2. Generate Tenant ID (if not provided)
   ├─ Slugify company name
   ├─ Add random suffix
   └─ Ensure uniqueness

3. Build Helm Values
   ├─ Apply tier resource limits
   ├─ Configure domains
   ├─ Set database credentials
   └─ Configure billing integration

4. Deploy Infrastructure
   ├─ Install Helm chart
   ├─ Create namespace: tenant-{id}
   ├─ Deploy PostgreSQL cluster
   ├─ Deploy API service
   ├─ Deploy Web frontend
   └─ Configure Ingress + TLS

5. Wait for Readiness
   ├─ PostgreSQL cluster ready (up to 5 min)
   ├─ API pods ready (up to 5 min)
   └─ Web pods ready (up to 5 min)

6. Return Credentials
   ├─ Tenant URL
   ├─ API URL
   ├─ Admin username/password
   └─ Resource allocation summary
```

## Kubernetes Resources Created

For tenant ID `acme-auto`:

```
Namespace: tenant-acme-auto

PostgreSQL Cluster:
├─ StatefulSet: acme-auto-postgres
├─ Service: acme-auto-postgres-rw (read-write)
├─ Service: acme-auto-postgres-ro (read-only)
├─ PVC: acme-auto-postgres-1 (50Gi)
└─ Secret: acme-auto-postgres-app (credentials)

API Service:
├─ Deployment: acme-auto-api (2 replicas)
├─ Service: acme-auto-api (ClusterIP)
└─ ConfigMap: acme-auto-api-config

Web Service:
├─ Deployment: acme-auto-web (2 replicas)
├─ Service: acme-auto-web (ClusterIP)
└─ ConfigMap: acme-auto-web-config

Ingress:
├─ Ingress: acme-auto-ingress
├─ Certificate: acme-auto-tls (Let's Encrypt)
└─ Hosts: acme-auto.mechanicbuddy.app, shop.acmeauto.com
```

## Custom Domains

To use a custom domain:

1. **Add to provisioning request:**
```json
{
  "customDomain": "shop.acmeauto.com"
}
```

2. **Configure DNS:**
```
shop.acmeauto.com. IN CNAME mechanicbuddy.app.
```

3. **TLS certificate** is automatically provisioned via Let's Encrypt.

## Resource Overrides (Enterprise)

For custom enterprise deployments:

```json
{
  "subscriptionTier": "enterprise",
  "resourceOverrides": {
    "postgresInstances": 5,        // HA with 5 replicas
    "postgresStorageSize": "500Gi", // Increased storage
    "apiReplicas": 10,              // High traffic
    "webReplicas": 5,
    "mechanicLimit": 100,           // Large organization
    "storageClass": "premium-ssd"   // Fast storage
  }
}
```

## Monitoring Provisioning

Watch pod creation in real-time:

```bash
# Watch all resources in tenant namespace
kubectl get all -n tenant-acme-auto -w

# Watch specific pods
kubectl get pods -n tenant-acme-auto -w

# Stream logs from API pod
kubectl logs -n tenant-acme-auto -l app.kubernetes.io/component=api -f

# Check PostgreSQL cluster status
kubectl get cluster -n tenant-acme-auto
```

## Troubleshooting

### Provisioning Stuck

```bash
# Check provisioning logs
kubectl logs -n mechanicbuddy-system -l app=management-api -f

# Check Helm release status
helm status tenant-acme-auto -n tenant-acme-auto

# Check pod events
kubectl describe pod -n tenant-acme-auto <pod-name>

# Check ingress
kubectl describe ingress -n tenant-acme-auto
```

### Database Not Ready

```bash
# Check PostgreSQL cluster
kubectl get cluster -n tenant-acme-auto

# Check PostgreSQL logs
kubectl logs -n tenant-acme-auto acme-auto-postgres-1

# Describe cluster
kubectl describe cluster -n tenant-acme-auto
```

### API/Web Pods Failing

```bash
# Check pod status
kubectl get pods -n tenant-acme-auto

# Get pod logs
kubectl logs -n tenant-acme-auto <pod-name>

# Check resource constraints
kubectl top pods -n tenant-acme-auto

# Describe pod
kubectl describe pod -n tenant-acme-auto <pod-name>
```

### Clean Up Failed Provisioning

```bash
# Delete namespace (removes all resources)
kubectl delete namespace tenant-acme-auto

# Or use the API
curl -X DELETE http://localhost:5000/api/tenants/acme-auto
```

## Environment Variables

Additional environment variables can be passed to tenant containers:

```json
{
  "additionalEnvVars": {
    "SMTP_HOST": "smtp.acmeauto.com",
    "SMTP_PORT": "587",
    "FEATURE_ADVANCED_REPORTS": "true",
    "TIMEZONE": "America/New_York"
  }
}
```

These are injected into both API and Web containers.

## Security Notes

1. **Admin Password**: Change immediately after first login
2. **Database Credentials**: Auto-generated by PostgreSQL operator
3. **TLS Certificates**: Automatically managed by cert-manager
4. **Network Isolation**: Each tenant in separate namespace
5. **Resource Limits**: Enforced to prevent noisy neighbors
6. **RBAC**: Tenants cannot access other tenant namespaces

## Best Practices

1. **Use Custom Domain** for professional/enterprise tiers
2. **Enable Stripe Integration** for paid subscriptions
3. **Monitor Resource Usage** to detect tier upgrades
4. **Set Mechanic Limits** based on subscription
5. **Enable Backups** for professional+ tiers
6. **Test Provisioning** in dev cluster first
7. **Document Custom Overrides** for enterprise tenants

## API Examples

### cURL

```bash
# Provision tenant
curl -X POST http://localhost:5000/api/tenants \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "Acme Auto Repair",
    "ownerEmail": "owner@acmeauto.com",
    "ownerFirstName": "John",
    "ownerLastName": "Doe",
    "subscriptionTier": "professional"
  }'

# Get status
curl http://localhost:5000/api/tenants/acme-auto-a1b2c3/status

# Deprovision
curl -X DELETE http://localhost:5000/api/tenants/acme-auto-a1b2c3
```

### C# Client

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

var request = new TenantProvisioningRequest
{
    CompanyName = "Acme Auto Repair",
    OwnerEmail = "owner@acmeauto.com",
    OwnerFirstName = "John",
    OwnerLastName = "Doe",
    SubscriptionTier = "professional"
};

var response = await client.PostAsJsonAsync("/api/tenants", request);
var result = await response.Content.ReadFromJsonAsync<TenantProvisioningResult>();

Console.WriteLine($"Tenant URL: {result.TenantUrl}");
```

### JavaScript/TypeScript

```typescript
const response = await fetch('http://localhost:5000/api/tenants', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    companyName: 'Acme Auto Repair',
    ownerEmail: 'owner@acmeauto.com',
    ownerFirstName: 'John',
    ownerLastName: 'Doe',
    subscriptionTier: 'professional'
  })
});

const result = await response.json();
console.log('Tenant URL:', result.tenantUrl);
```
