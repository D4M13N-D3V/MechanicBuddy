# Tenant Provisioning Architecture

## System Overview

## Two Provisioning Paths

The `TenantProvisioningService` implements two distinct provisioning workflows based on subscription tier and configuration:

### Path Decision Flow

```
                            Provisioning Request
                                     │
                                     ▼
                    ┌────────────────────────────────┐
                    │  Check Subscription Tier       │
                    │  & FreeTier.Enabled Config     │
                    └────────────┬───────────────────┘
                                 │
                    ┌────────────┴─────────────┐
                    │                          │
          ┌─────────▼──────────┐    ┌──────────▼──────────┐
          │ Tier: "free" OR    │    │ Tier: "professional"│
          │       "demo"       │    │       "enterprise"  │
          │ AND                │    │ OR                  │
          │ FreeTier.Enabled   │    │ FreeTier.Enabled=   │
          │      = true        │    │      false          │
          └─────────┬──────────┘    └──────────┬──────────┘
                    │                          │
                    ▼                          ▼
     ┌──────────────────────────┐   ┌───────────────────────────┐
     │  PATH 1: FREE TIER       │   │  PATH 2: DEDICATED        │
     │  ProvisionFreeTier       │   │  ProvisionDedicatedTenant │
     │  TenantAsync()           │   │  Async()                  │
     └──────────────────────────┘   └───────────────────────────┘
```

### Path 1: Free/Demo Tier Provisioning

**Trigger Conditions:**
- `subscriptionTier` is "free" OR "demo"
- AND `ProvisioningOptions.FreeTier.Enabled = true`

**Workflow Steps:**

```
┌─────────────────────────────────────────────────────────────────┐
│  1. Create Database Schema                                      │
│     - Connect to shared PostgreSQL cluster                      │
│     - Create tenant-specific schema: "tenant_{tenantId}"        │
│     - Create application user with schema-scoped permissions    │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. Run Database Migrations                                     │
│     - Execute DbUp scripts against new schema                   │
│     - Seed default data (admin user, initial settings)          │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. Configure NPM Routing                                       │
│     - Add hostname-based route to NPM (Nginx Proxy Manager)     │
│     - Route: {tenantId}.mechanicbuddy.app → shared API/Web      │
│     - Configure X-Tenant-ID header injection                    │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. Update Tenant Record                                        │
│     - Set DeploymentMode = "shared"                             │
│     - Set Namespace = "mechanicbuddy-free-tier"                 │
│     - Set TenantUrl = "https://{tenantId}.mechanicbuddy.app"    │
│     - Set Status = "Active"                                     │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
                  ┌──────────────┐
                  │  COMPLETED   │
                  │  (No Helm)   │
                  └──────────────┘
```

**Key Characteristics:**
- **No Kubernetes namespace created** - tenant added to existing `mechanicbuddy-free-tier` namespace
- **No Helm deployment** - uses pre-deployed shared infrastructure
- **Shared resources** - PostgreSQL, API, and Web services shared across all free-tier tenants
- **Isolation via database schema** - each tenant has isolated data in separate PostgreSQL schema
- **Hostname-based routing** - NPM routes requests based on subdomain
- **Fast provisioning** - typically < 30 seconds (no container startup time)
- **Resource efficient** - minimal overhead per tenant

**Configuration Options:**
```json
{
  "Provisioning": {
    "FreeTier": {
      "Enabled": true,
      "Namespace": "mechanicbuddy-free-tier",
      "MaxTenantsPerInstance": 100,
      "PostgresConnectionString": "Host=shared-postgres;Database=mechanicbuddy_shared",
      "ApiUrl": "http://api.mechanicbuddy-free-tier.svc.cluster.local",
      "WebUrl": "http://web.mechanicbuddy-free-tier.svc.cluster.local"
    }
  }
}
```

**Shared Infrastructure (pre-deployed):**

```
┌──────────────────────────────────────────────────────────────────┐
│  Namespace: mechanicbuddy-free-tier                              │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Shared PostgreSQL Cluster                                 │ │
│  │  - Single database with multiple schemas                   │ │
│  │  - Schema per tenant: "tenant_{tenantId}"                  │ │
│  │  - Resource limits: 4Gi memory, 2 CPU                      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Shared API Deployment (3 replicas)                        │ │
│  │  - Multi-tenant aware                                      │ │
│  │  - Resolves tenant from X-Tenant-ID header or hostname     │ │
│  │  - Connects to appropriate schema                          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Shared Web Deployment (3 replicas)                        │ │
│  │  - Multi-tenant aware                                      │ │
│  │  - Passes tenant context to API                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  NPM (Nginx Proxy Manager)                                 │ │
│  │  - Hostname-based routing                                  │ │
│  │  - Automatic SSL via Let's Encrypt                         │ │
│  │  - X-Tenant-ID header injection                            │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

---

### Path 2: Dedicated Tier Provisioning

**Trigger Conditions:**
- `subscriptionTier` is "professional" OR "enterprise"
- OR `ProvisioningOptions.FreeTier.Enabled = false` (forces dedicated for all tiers)

**Workflow Steps:**

```
┌─────────────────────────────────────────────────────────────────┐
│  1. Create Kubernetes Namespace                                 │
│     - Namespace: "tenant-{tenantId}"                            │
│     - Labels: tier={tier}, managed-by=mechanicbuddy            │
│     - Resource quotas applied based on tier                     │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  2. Build Helm Values                                           │
│     - Generate tenant-specific configuration                    │
│     - Apply tier-based resource limits                          │
│     - Configure domain, TLS, backup settings                    │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  3. Deploy Helm Chart                                           │
│     - Chart: mechanicbuddy-tenant                               │
│     - Release: "tenant-{tenantId}"                              │
│     - Creates: PostgreSQL cluster, API, Web, Ingress, Secrets   │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  4. Wait for PostgreSQL Ready                                   │
│     - Poll pod status every 5s                                  │
│     - Timeout: 5 minutes                                        │
│     - Checks: Pod Running AND Ready                             │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  5. Wait for API Ready                                          │
│     - Poll pod status every 5s                                  │
│     - Timeout: 5 minutes                                        │
│     - Checks: All replicas Running AND Ready                    │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  6. Wait for Web Ready                                          │
│     - Poll pod status every 5s                                  │
│     - Timeout: 5 minutes                                        │
│     - Checks: All replicas Running AND Ready                    │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  7. Configure Ingress & TLS                                     │
│     - Hostname: {tenantId}.mechanicbuddy.app (or custom)        │
│     - TLS certificate via cert-manager (Let's Encrypt)          │
│     - Routes: / → Web, /api → API                               │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  8. Update Tenant Record                                        │
│     - Set DeploymentMode = "dedicated"                          │
│     - Set Namespace = "tenant-{tenantId}"                       │
│     - Set HelmRelease = "tenant-{tenantId}"                     │
│     - Set TenantUrl, ApiUrl                                     │
│     - Set Status = "Active"                                     │
└────────────────────────┬────────────────────────────────────────┘
                         ▼
                  ┌──────────────┐
                  │  COMPLETED   │
                  │  (Isolated)  │
                  └──────────────┘
```

**Key Characteristics:**
- **Dedicated namespace** - complete isolation at Kubernetes level
- **Dedicated PostgreSQL cluster** - no data sharing with other tenants
- **Dedicated API/Web instances** - independent scaling and updates
- **Full resource allocation** - CPU, memory, storage per tier limits
- **High availability** - multiple replicas (configurable by tier)
- **Custom domains** - optional customer-specific hostnames
- **Backup & disaster recovery** - tenant-specific backup schedules
- **Slower provisioning** - typically 2-5 minutes (container startup, DB initialization)

**Configuration Options:**
```json
{
  "Provisioning": {
    "TierResourceLimits": {
      "professional": {
        "PostgresInstances": 1,
        "PostgresStorage": "50Gi",
        "PostgresMemory": "2Gi",
        "ApiReplicas": 2,
        "ApiMemory": "1Gi",
        "WebReplicas": 2,
        "WebMemory": "512Mi",
        "MechanicLimit": 20,
        "BackupEnabled": true
      },
      "enterprise": {
        "PostgresInstances": 3,
        "PostgresStorage": "200Gi",
        "PostgresMemory": "8Gi",
        "ApiReplicas": 3,
        "ApiMemory": "4Gi",
        "WebReplicas": 3,
        "WebMemory": "2Gi",
        "MechanicLimit": -1,
        "BackupEnabled": true,
        "BackupRetention": "30d"
      }
    }
  }
}
```

**Dedicated Infrastructure (created per tenant):**

```
┌──────────────────────────────────────────────────────────────────┐
│  Namespace: tenant-acme-auto-a1b2c3                              │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Dedicated PostgreSQL Cluster (CloudNativePG)              │ │
│  │  - High Availability: 1-3 instances (tier-based)           │ │
│  │  - Storage: 50Gi-200Gi (tier-based)                        │ │
│  │  - Automatic backups & point-in-time recovery              │ │
│  │  - PVC: Persistent storage                                 │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Dedicated API Deployment                                  │ │
│  │  - Replicas: 2-3 (tier-based)                              │ │
│  │  - Memory: 1Gi-4Gi per pod                                 │ │
│  │  - Single-tenant mode (no tenant resolution)               │ │
│  │  - Direct PostgreSQL connection                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Dedicated Web Deployment                                  │ │
│  │  - Replicas: 2-3 (tier-based)                              │ │
│  │  - Memory: 512Mi-2Gi per pod                               │ │
│  │  - Single-tenant mode                                      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Ingress (NGINX)                                           │ │
│  │  - Host: acme-auto-a1b2c3.mechanicbuddy.app               │ │
│  │  - TLS: Let's Encrypt certificate                          │ │
│  │  - Routes: / → Web, /api → API                             │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  Secrets                                                   │ │
│  │  - postgres-app: Dedicated DB credentials                  │ │
│  │  - jwt-secret: Tenant-specific JWT key                     │ │
│  │  - smtp-credentials: Email configuration                   │ │
│  └────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

---

## Comparison Matrix

| Feature                        | Free/Demo Tier (Path 1)         | Dedicated Tier (Path 2)           |
|--------------------------------|---------------------------------|-----------------------------------|
| **Kubernetes Namespace**       | Shared (mechanicbuddy-free-tier)| Dedicated (tenant-{id})          |
| **Helm Deployment**            | No (uses pre-deployed)          | Yes (full chart deployment)      |
| **PostgreSQL**                 | Shared cluster, isolated schema | Dedicated cluster                |
| **API/Web Instances**          | Shared pods (multi-tenant)      | Dedicated pods (single-tenant)   |
| **Resource Allocation**        | Shared (limited per-tenant)     | Dedicated (tier-based limits)    |
| **Provisioning Time**          | ~30 seconds                     | ~2-5 minutes                     |
| **High Availability**          | Limited (shared resources)      | Full HA (multiple replicas)      |
| **Data Isolation**             | Schema-level                    | Cluster-level                    |
| **Custom Domains**             | Not supported                   | Supported                        |
| **Backup/Recovery**            | Shared backup                   | Dedicated backup schedule        |
| **Scaling**                    | Vertical (add more free-tier)   | Horizontal (per-tenant scaling)  |
| **Cost per Tenant**            | Very low (minimal overhead)     | Higher (dedicated resources)     |
| **Tenant Limit per Instance**  | ~100 (configurable)             | 1                                |
| **Upgrade Path**               | Can migrate to dedicated        | Can downgrade to shared          |

---

## Deployment Mode Tracking

The `Tenant` entity includes a `DeploymentMode` field to track which provisioning path was used:

```csharp
public class Tenant
{
    public string Id { get; set; }
    public string CompanyName { get; set; }
    public string SubscriptionTier { get; set; }

    // Provisioning metadata
    public string DeploymentMode { get; set; }  // "shared" or "dedicated"
    public string Namespace { get; set; }        // K8s namespace (or "mechanicbuddy-free-tier")
    public string HelmRelease { get; set; }      // null for shared, release name for dedicated

    // URLs
    public string TenantUrl { get; set; }
    public string ApiUrl { get; set; }

    // Status
    public string Status { get; set; }  // "Provisioning", "Active", "Suspended", "Deprovisioned"
}
```

**Usage in Upgrade/Downgrade Flows:**

```csharp
// Upgrade from free to professional
if (tenant.DeploymentMode == "shared" && newTier == "professional")
{
    // 1. Provision dedicated infrastructure
    await ProvisionDedicatedTenantAsync(tenant);

    // 2. Migrate data from shared schema to dedicated database
    await MigrateTenantDataAsync(tenant);

    // 3. Update NPM to remove shared routing
    await RemoveNpmRoutingAsync(tenant);

    // 4. Update tenant record
    tenant.DeploymentMode = "dedicated";
    tenant.Namespace = $"tenant-{tenant.Id}";
}

// Downgrade from professional to free
if (tenant.DeploymentMode == "dedicated" && newTier == "free")
{
    // 1. Create schema in shared database
    await CreateSharedSchemaAsync(tenant);

    // 2. Migrate data from dedicated to shared
    await MigrateTenantDataAsync(tenant);

    // 3. Configure NPM routing
    await ConfigureNpmRoutingAsync(tenant);

    // 4. Deprovision dedicated infrastructure
    await DeprovisionDedicatedTenantAsync(tenant);

    // 5. Update tenant record
    tenant.DeploymentMode = "shared";
    tenant.Namespace = "mechanicbuddy-free-tier";
    tenant.HelmRelease = null;
}
```

**Usage in Deprovisioning:**

```csharp
public async Task DeprovisionTenantAsync(string tenantId)
{
    var tenant = await GetTenantAsync(tenantId);

    if (tenant.DeploymentMode == "dedicated")
    {
        // Full namespace deletion
        await helmService.UninstallAsync(tenant.HelmRelease, tenant.Namespace);
        await k8sClient.DeleteNamespaceAsync(tenant.Namespace);
    }
    else if (tenant.DeploymentMode == "shared")
    {
        // Remove from shared infrastructure
        await RemoveNpmRoutingAsync(tenant);
        await DropSchemaAsync(tenant);
    }

    tenant.Status = "Deprovisioned";
    await UpdateTenantAsync(tenant);
}
```

---

## High-Level System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Frontend / API Client                        │
│                    (Management Dashboard / CLI)                      │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
                                │ HTTP POST /api/provisioning/provision
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│                    Management API (.NET 9)                           │
│                                                                      │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │           ProvisioningController                           │    │
│  │  - Authorization (SuperAdminOnly)                          │    │
│  │  - Request validation                                      │    │
│  │  - Error handling                                          │    │
│  └───────────────────────────┬────────────────────────────────┘    │
│                              │                                      │
│  ┌───────────────────────────▼────────────────────────────────┐    │
│  │       TenantProvisioningService                            │    │
│  │  - Request validation                                      │    │
│  │  - Tenant ID generation                                    │    │
│  │  - PATH ROUTING: Free vs Dedicated                         │    │
│  │  - Orchestration logic                                     │    │
│  │  - Status monitoring                                       │    │
│  └────┬───────────────────────────────────────────────────┬───┘    │
│       │                                                   │        │
│       │ (Free Tier)                         (Dedicated)   │        │
│       │                                                   │        │
│  ┌────▼────────────────┐                    ┌────────────▼──────┐ │
│  │ DatabaseService     │                    │   HelmService     │ │
│  │ - Schema creation   │                    │  - helm install   │ │
│  │ - Migrations        │                    │  - helm upgrade   │ │
│  │ - NPM routing       │                    │  - helm uninstall │ │
│  └─────────────────────┘                    └───────────────────┘ │
│                                                   │                │
│                                    ┌──────────────▼──────────────┐ │
│                                    │ KubernetesClientService     │ │
│                                    │  - Namespace operations     │ │
│                                    │  - Pod monitoring           │ │
│                                    │  - Secret management        │ │
│                                    │  - Ingress queries          │ │
│                                    └─────────────────────────────┘ │
└──────────────────────┬─────────────────────────┬───────────────────┘
                       │                         │
                       │ PostgreSQL/NPM          │ helm CLI / K8s API
                       │                         │
┌──────────────────────▼─────────┐   ┌───────────▼───────────────────┐
│  Shared Infrastructure          │   │  Kubernetes Cluster           │
│  (mechanicbuddy-free-tier)     │   │                               │
│                                 │   │  ┌─────────────────────────┐ │
│  - Shared PostgreSQL            │   │  │ Namespace: tenant-xxx   │ │
│  - Shared API pods              │   │  │                         │ │
│  - Shared Web pods              │   │  │ - Dedicated PostgreSQL  │ │
│  - NPM routing                  │   │  │ - Dedicated API         │ │
│                                 │   │  │ - Dedicated Web         │ │
└─────────────────────────────────┘   │  │ - Ingress + TLS         │ │
                                      │  └─────────────────────────┘ │
                                      └──────────────────────────────┘
```

## Provisioning Sequence Diagrams

### Dedicated Tier Provisioning Sequence (Path 2)

```
Client          Controller       Provisioning      Helm          K8s
  │                 │             Service          Service       Client
  │                 │                │                │            │
  │─────POST────────>               │                │            │
  │ Provision       │                │                │            │
  │ (professional)  │                │                │            │
  │                 │                │                │            │
  │                 │────Validate────>               │            │
  │                 │                │                │            │
  │                 │                │────Check────────────────────>
  │                 │                │   Cluster    │            │
  │                 │                │<───Access────────────────────
  │                 │                │   OK         │            │
  │                 │                │                │            │
  │                 │                │───Check─────────>          │
  │                 │                │   Helm      │              │
  │                 │                │<──Available────              │
  │                 │                │             │              │
  │                 │<───Valid───────                │            │
  │                 │                │                │            │
  │                 │─Generate ID────>               │            │
  │                 │<──acme-auto────                │            │
  │                 │                │                │            │
  │                 │─Build Values───>               │            │
  │                 │<──YAML─────────                │            │
  │                 │                │                │            │
  │                 │─Deploy─────────>               │            │
  │                 │                │───Install───────>          │
  │                 │                │   Helm      │              │
  │                 │                │   Chart     │              │
  │                 │                │             │              │
  │                 │                │             │──Create──────>
  │                 │                │             │ Namespace    │
  │                 │                │             │              │
  │                 │                │             │──Deploy──────>
  │                 │                │             │ PostgreSQL   │
  │                 │                │             │              │
  │                 │                │             │──Deploy──────>
  │                 │                │             │ API          │
  │                 │                │             │              │
  │                 │                │             │──Deploy──────>
  │                 │                │             │ Web          │
  │                 │                │             │              │
  │                 │                │<──Success────              │
  │                 │                │             │              │
  │                 │─Wait DB────────>             │              │
  │                 │                │──────────────────Get Pods──>
  │                 │                │<─────────────────Status─────
  │                 │                │ (polling every 5s)          │
  │                 │                │──────────────────Get Pods──>
  │                 │                │<─────────────────Ready──────
  │                 │<──DB Ready─────                │            │
  │                 │                │                │            │
  │                 │─Wait API───────>               │            │
  │                 │                │──────────────────Get Pods──>
  │                 │                │<─────────────────Ready──────
  │                 │<──API Ready────                │            │
  │                 │                │                │            │
  │                 │─Wait Web───────>               │            │
  │                 │                │──────────────────Get Pods──>
  │                 │                │<─────────────────Ready──────
  │                 │<──Web Ready────                │            │
  │                 │                │                │            │
  │                 │<──Result───────                │            │
  │<────200 OK──────                 │                │            │
  │ {success, url,  │                │                │            │
  │  credentials,   │                │                │            │
  │  mode=dedicated}│                │                │            │
  │                 │                │                │            │
```

### Free Tier Provisioning Sequence (Path 1)

```
Client          Controller       Provisioning      Database      NPM
  │                 │             Service          Service       API
  │                 │                │                │            │
  │─────POST────────>               │                │            │
  │ Provision       │                │                │            │
  │ (free)          │                │                │            │
  │                 │                │                │            │
  │                 │────Validate────>               │            │
  │                 │                │                │            │
  │                 │<───Valid───────                │            │
  │                 │                │                │            │
  │                 │─Generate ID────>               │            │
  │                 │<──acme-auto────                │            │
  │                 │                │                │            │
  │                 │─Create Schema──>               │            │
  │                 │                │──Connect──────>            │
  │                 │                │  (shared-pg)               │
  │                 │                │               │            │
  │                 │                │──CREATE SCHEMA─>           │
  │                 │                │  tenant_xxx    │           │
  │                 │                │               │            │
  │                 │                │<──Success─────             │
  │                 │<──Schema Ready─                │            │
  │                 │                │                │            │
  │                 │─Run Migrations─>               │            │
  │                 │                │──DbUp Scripts─>            │
  │                 │                │<──Complete────             │
  │                 │<──Migrated─────                │            │
  │                 │                │                │            │
  │                 │─Configure Route>               │            │
  │                 │                │──────────────────Add Route─>
  │                 │                │                │  (NPM API) │
  │                 │                │                │            │
  │                 │                │                │  Create:   │
  │                 │                │                │  Host: xxx │
  │                 │                │                │  Target:   │
  │                 │                │                │   shared   │
  │                 │                │                │  Header:   │
  │                 │                │                │   X-Tenant │
  │                 │                │<─────────────────Success───
  │                 │<──Route Ready──                │            │
  │                 │                │                │            │
  │                 │<──Result───────                │            │
  │<────200 OK──────                 │                │            │
  │ {success, url,  │                │                │            │
  │  credentials,   │                │                │            │
  │  mode=shared}   │                │                │            │
  │                 │                │                │            │
```

## Component Interaction

### Dedicated Tier Component Flow (Path 2)

```
┌────────────────────────────────────────────────────────────────┐
│                   TenantProvisioningService                     │
│                    (Dedicated Path)                             │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Step 1: Validation                                      │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Validate   │→ │   Check     │→ │   Validate     │  │  │
│  │  │   Request    │  │   K8s/Helm  │  │   Uniqueness   │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 2-3: ID Generation & Values Building              │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Generate   │→ │   Apply     │→ │   Generate     │  │  │
│  │  │   Tenant ID  │  │   Tier      │  │   YAML         │  │  │
│  │  └──────────────┘  │   Limits    │  │   Values       │  │  │
│  │                    └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 4: Helm Deployment                                │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Write      │→ │   Execute   │→ │   Monitor      │  │  │
│  │  │   Temp File  │  │   helm      │  │   Output       │  │  │
│  │  └──────────────┘  │   install   │  └────────────────┘  │  │
│  │                    └─────────────┘                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 5-7: Readiness Checks                             │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Wait for   │→ │   Wait for  │→ │   Wait for     │  │  │
│  │  │   PostgreSQL │  │   API Pods  │  │   Web Pods     │  │  │
│  │  │   (5 min)    │  │   (5 min)   │  │   (5 min)      │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  │       │                   │                 │            │  │
│  │       │   Poll every 5s   │                 │            │  │
│  │       └───────────────────┴─────────────────┘            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 8: Finalization                                   │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Set URLs   │→ │   Set Mode  │→ │   Build        │  │  │
│  │  │   & Creds    │  │  "dedicated"│  │   Result       │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

### Free Tier Component Flow (Path 1)

```
┌────────────────────────────────────────────────────────────────┐
│                   TenantProvisioningService                     │
│                      (Free Tier Path)                           │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Step 1: Validation                                      │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Validate   │→ │   Check     │→ │   Validate     │  │  │
│  │  │   Request    │  │   Capacity  │  │   Uniqueness   │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 2: ID Generation                                  │  │
│  │  ┌──────────────┐  ┌─────────────┐                      │  │
│  │  │   Generate   │→ │   Create    │                      │  │
│  │  │   Tenant ID  │  │   Creds     │                      │  │
│  │  └──────────────┘  └─────────────┘                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 3: Database Setup                                 │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Create     │→ │   Run       │→ │   Seed         │  │  │
│  │  │   Schema     │  │  Migrations │  │   Data         │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 4: NPM Routing Configuration                      │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Create     │→ │   Configure │→ │   Enable       │  │  │
│  │  │   Host Route │  │  X-Tenant   │  │   SSL          │  │  │
│  │  └──────────────┘  │   Header    │  └────────────────┘  │  │
│  │                    └─────────────┘                      │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│  ┌──────────────────────────▼───────────────────────────────┐  │
│  │  Step 5: Finalization                                   │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Set URLs   │→ │   Set Mode  │→ │   Build        │  │  │
│  │  │   & Creds    │  │   "shared"  │  │   Result       │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

## Data Flow

### Dedicated Tier Data Flow (Path 2)

```
Request DTO
    │
    ├─> TenantProvisioningRequest
    │   ├─ companyName: "Acme Auto Repair"
    │   ├─ subscriptionTier: "professional"
    │   ├─ ownerEmail: "owner@acmeauto.com"
    │   └─ customDomain: "shop.acmeauto.com"
    │
    ▼
Validation
    │
    ├─> ValidationResult
    │   ├─ isValid: true
    │   ├─ errors: []
    │   └─ warnings: []
    │
    ▼
ID Generation
    │
    ├─> tenantId: "acme-auto-a1b2c3"
    │   namespace: "tenant-acme-auto-a1b2c3"
    │   helmRelease: "tenant-acme-auto-a1b2c3"
    │
    ▼
Helm Values
    │
    ├─> YAML Configuration
    │   ├─ tenant.id: "acme-auto-a1b2c3"
    │   ├─ domains.default: "acme-auto-a1b2c3.mechanicbuddy.app"
    │   ├─ postgresql.instances: 1
    │   ├─ postgresql.storage.size: "50Gi"
    │   ├─ api.replicas: 2
    │   ├─ api.resources.limits.memory: "1Gi"
    │   └─ billing.mechanicLimit: 20
    │
    ▼
Helm Execution
    │
    ├─> Process
    │   ├─ Command: helm install
    │   ├─ Args: [tenant-acme-auto-a1b2c3, ./chart, -f values.yaml, ...]
    │   └─ Output: Release "tenant-acme-auto-a1b2c3" created
    │
    ▼
Kubernetes Resources
    │
    ├─> Created in Cluster
    │   ├─ Namespace: tenant-acme-auto-a1b2c3
    │   ├─ PostgreSQL Cluster (1 instance)
    │   ├─ API Deployment (2 replicas)
    │   ├─ Web Deployment (2 replicas)
    │   ├─ Ingress (with TLS)
    │   └─ Secrets (credentials)
    │
    ▼
Readiness Monitoring
    │
    ├─> Pod Status Polling
    │   ├─ PostgreSQL: [Running, Ready] ✓
    │   ├─ API Pod 1: [Running, Ready] ✓
    │   ├─ API Pod 2: [Running, Ready] ✓
    │   ├─ Web Pod 1: [Running, Ready] ✓
    │   └─ Web Pod 2: [Running, Ready] ✓
    │
    ▼
Result DTO
    │
    └─> TenantProvisioningResult
        ├─ success: true
        ├─ tenantId: "acme-auto-a1b2c3"
        ├─ tenantUrl: "https://acme-auto-a1b2c3.mechanicbuddy.app"
        ├─ apiUrl: "https://acme-auto-a1b2c3.mechanicbuddy.app/api"
        ├─ namespace: "tenant-acme-auto-a1b2c3"
        ├─ deploymentMode: "dedicated"
        ├─ adminUsername: "admin"
        ├─ adminPassword: "ChangeMeOnFirstLogin!"
        ├─ helmRelease: "tenant-acme-auto-a1b2c3"
        ├─ subscriptionTier: "professional"
        ├─ provisioningDuration: "00:02:45"
        ├─ resources:
        │   ├─ postgresInstances: 1
        │   ├─ postgresStorage: "50Gi"
        │   ├─ apiReplicas: 2
        │   ├─ webReplicas: 2
        │   ├─ mechanicLimit: 20
        │   └─ backupEnabled: true
        └─ provisioningLog: [13 entries with timestamps]
```

### Free Tier Data Flow (Path 1)

```
Request DTO
    │
    ├─> TenantProvisioningRequest
    │   ├─ companyName: "Quick Fix Garage"
    │   ├─ subscriptionTier: "free"
    │   └─ ownerEmail: "owner@quickfix.com"
    │
    ▼
Validation
    │
    ├─> ValidationResult
    │   ├─ isValid: true
    │   ├─ errors: []
    │   └─ warnings: ["Limited to 5 users", "No custom domain"]
    │
    ▼
ID Generation
    │
    ├─> tenantId: "quick-fix-x7y8z9"
    │   namespace: "mechanicbuddy-free-tier" (shared)
    │   helmRelease: null (no Helm deployment)
    │
    ▼
Database Schema Creation
    │
    ├─> SQL Commands
    │   ├─ CREATE SCHEMA "tenant_quick_fix_x7y8z9"
    │   ├─ CREATE USER "tenant_quick_fix_x7y8z9_app"
    │   ├─ GRANT USAGE ON SCHEMA TO app_user
    │   └─ SET search_path = tenant_quick_fix_x7y8z9
    │
    ▼
Database Migrations
    │
    ├─> DbUp Execution
    │   ├─ Context: Schema "tenant_quick_fix_x7y8z9"
    │   ├─ Scripts: [001_CreateTables.sql, 002_CreateIndexes.sql, ...]
    │   └─ Seed: Default admin user, settings
    │
    ▼
NPM Routing Configuration
    │
    ├─> NPM API Call
    │   ├─ Host: "quick-fix-x7y8z9.mechanicbuddy.app"
    │   ├─ Target: http://api.mechanicbuddy-free-tier.svc.cluster.local
    │   ├─ SSL: true (Let's Encrypt)
    │   ├─ Headers:
    │   │   └─ X-Tenant-ID: "quick-fix-x7y8z9"
    │   └─ Response: { proxyHostId: 123 }
    │
    ▼
Result DTO
    │
    └─> TenantProvisioningResult
        ├─ success: true
        ├─ tenantId: "quick-fix-x7y8z9"
        ├─ tenantUrl: "https://quick-fix-x7y8z9.mechanicbuddy.app"
        ├─ apiUrl: "https://quick-fix-x7y8z9.mechanicbuddy.app/api"
        ├─ namespace: "mechanicbuddy-free-tier"
        ├─ deploymentMode: "shared"
        ├─ adminUsername: "admin"
        ├─ adminPassword: "ChangeMeOnFirstLogin!"
        ├─ helmRelease: null
        ├─ subscriptionTier: "free"
        ├─ provisioningDuration: "00:00:28"
        ├─ resources:
        │   ├─ postgresInstances: "shared"
        │   ├─ postgresStorage: "shared"
        │   ├─ apiReplicas: "shared"
        │   ├─ webReplicas: "shared"
        │   ├─ mechanicLimit: 5
        │   └─ backupEnabled: false
        └─ provisioningLog: [5 entries with timestamps]
```

## Error Handling Flow

```
┌─────────────────────┐
│  Request Received   │
└──────────┬──────────┘
           │
           ▼
    ┌──────────────┐
    │  Validation  │──── Fail ────> Return 400 BadRequest
    └──────┬───────┘                 { errors: [...] }
           │ Pass
           ▼
    ┌──────────────┐
    │ Check Tenant │──── Exists ──> Return 400 BadRequest
    │   ID Unique  │                 { error: "Already exists" }
    └──────┬───────┘
           │ Unique
           ▼
    ┌──────────────┐
    │  Deploy Helm │──── Fail ────> Return 400 BadRequest
    └──────┬───────┘                 { helmOutput: "..." }
           │ Success
           ▼
    ┌──────────────┐
    │  Wait for DB │──── Timeout ──> Cleanup Namespace
    └──────┬───────┘                 Return 500 Error
           │ Ready                   { error: "DB timeout" }
           ▼
    ┌──────────────┐
    │ Wait for API │──── Timeout ──> Log Warning
    └──────┬───────┘                 Continue (non-critical)
           │ Ready
           ▼
    ┌──────────────┐
    │ Wait for Web │──── Timeout ──> Log Warning
    └──────┬───────┘                 Continue (non-critical)
           │ Ready
           ▼
    ┌──────────────┐
    │Return Success│
    └──────────────┘
```

## Technology Stack

```
┌─────────────────────────────────────────────────────────────┐
│                        Application Layer                     │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  .NET 9 / ASP.NET Core                              │    │
│  │  - Controllers (REST API)                           │    │
│  │  - Dependency Injection                             │    │
│  │  - Configuration System                             │    │
│  │  - Logging (ILogger)                                │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                      Service Layer                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  TenantProvisioningService                          │    │
│  │  - Business Logic                                   │    │
│  │  - Orchestration                                    │    │
│  │  - State Management                                 │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
            ┌───────────────┴───────────────┐
            │                               │
┌───────────▼──────────┐       ┌────────────▼─────────────────┐
│  Infrastructure       │       │  Infrastructure              │
│  HelmService          │       │  KubernetesClientService     │
│                       │       │                              │
│  ┌─────────────────┐ │       │  ┌────────────────────────┐  │
│  │ System.         │ │       │  │  KubernetesClient      │  │
│  │ Diagnostics.    │ │       │  │  (Official SDK)        │  │
│  │ Process         │ │       │  │  - Core V1 API         │  │
│  └─────────────────┘ │       │  │  - Apps V1 API         │  │
└───────────┬──────────┘       │  │  - Networking V1 API   │  │
            │                  │  └────────────────────────┘  │
            │                  └──────────────┬────────────────┘
            │                                 │
┌───────────▼──────────┐       ┌──────────────▼────────────────┐
│  External Tool        │       │  External System              │
│  Helm 3 CLI           │       │  Kubernetes Cluster           │
│  - Install            │       │  - API Server                 │
│  - Upgrade            │       │  - etcd                       │
│  - Uninstall          │       │  - Scheduler                  │
│  - Status             │       │  - Controller Manager         │
└───────────┬──────────┘       └──────────────┬────────────────┘
            │                                  │
            └──────────────┬───────────────────┘
                           │
            ┌──────────────▼────────────────┐
            │  Kubernetes Resources          │
            │  - Namespaces                  │
            │  - Deployments                 │
            │  - StatefulSets                │
            │  - Services                    │
            │  - Ingresses                   │
            │  - Secrets                     │
            │  - ConfigMaps                  │
            │  - PersistentVolumeClaims      │
            └────────────────────────────────┘
```

## Deployment Architecture

```
┌───────────────────────────────────────────────────────────────────┐
│  Kubernetes Cluster                                               │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  Namespace: mechanicbuddy-system                         │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  Management API Deployment                         │ │    │
│  │  │  ┌──────────────────────────────────────────────┐  │ │    │
│  │  │  │  Pod: management-api                         │  │ │    │
│  │  │  │  - .NET 9 Runtime                            │  │ │    │
│  │  │  │  - TenantProvisioningService (PATH ROUTING)  │  │ │    │
│  │  │  │  - Helm CLI installed                        │  │ │    │
│  │  │  │  - ServiceAccount: mechanicbuddy-mgmt        │  │ │    │
│  │  │  └──────────────────────────────────────────────┘  │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  RBAC (ClusterRole + ClusterRoleBinding)          │ │    │
│  │  │  - namespaces: create, delete, get, list          │ │    │
│  │  │  - pods: get, list, watch                         │ │    │
│  │  │  - deployments: create, update, delete            │ │    │
│  │  │  - ingresses: create, update, delete              │ │    │
│  │  │  - postgresql.cnpg.io: all operations            │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  Namespace: mechanicbuddy-free-tier (SHARED)             │    │
│  │  Pre-deployed infrastructure for free/demo tenants      │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  Shared PostgreSQL Cluster                         │ │    │
│  │  │  - Multiple schemas (one per tenant)               │ │    │
│  │  │  - Schemas: tenant_xxx, tenant_yyy, tenant_zzz     │ │    │
│  │  │  - Capacity: ~100 tenants                          │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  Shared API Deployment (3 replicas)                │ │    │
│  │  │  - Multi-tenant aware                              │ │    │
│  │  │  - Resolves tenant from X-Tenant-ID or hostname    │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  Shared Web Deployment (3 replicas)                │ │    │
│  │  │  - Multi-tenant aware                              │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  │                                                          │    │
│  │  ┌────────────────────────────────────────────────────┐ │    │
│  │  │  NPM (Nginx Proxy Manager)                         │ │    │
│  │  │  - Dynamic host routing per tenant                 │ │    │
│  │  │  - X-Tenant-ID header injection                    │ │    │
│  │  │  - Let's Encrypt SSL per hostname                  │ │    │
│  │  └────────────────────────────────────────────────────┘ │    │
│  │                                                          │    │
│  │  Tenants: quick-fix-x7y8z9, joes-garage-a1b2c3, ...     │    │
│  │  (No Helm deployments, added dynamically via NPM/DB)    │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  Namespace: tenant-acme-auto-a1b2c3 (DEDICATED)          │    │
│  │  Professional tier - fully isolated infrastructure       │    │
│  │                                                          │    │
│  │  [PostgreSQL Cluster] [API x2] [Web x2] [Ingress] [...]  │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │  Namespace: tenant-enterprise-corp-x9y8z7 (DEDICATED)    │    │
│  │  Enterprise tier - HA with multiple instances            │    │
│  │                                                          │    │
│  │  [PostgreSQL HA x3] [API x3] [Web x3] [Ingress] [...]    │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

This architecture supports:

**Free Tier (Shared Model):**
- **Cost Efficiency**: Single set of resources serves many tenants
- **Fast Provisioning**: No Kubernetes resources to create (~30s)
- **Schema Isolation**: Each tenant has isolated database schema
- **Easy Scaling**: Add more tenants to shared instance (up to capacity limit)
- **Limited Resources**: Shared CPU/memory, suitable for small workloads

**Dedicated Tier (Isolated Model):**
- **Complete Isolation**: Each tenant in separate namespace with own resources
- **Guaranteed Resources**: CPU, memory, storage dedicated per tier
- **High Availability**: Multiple replicas, independent scaling
- **Custom Configuration**: Per-tenant resource limits, backup policies
- **Production Grade**: Suitable for mission-critical workloads

**Management API:**
- **Path Routing**: Automatically selects free or dedicated provisioning
- **Configuration Driven**: `FreeTier.Enabled` toggle controls behavior
- **Unified Interface**: Same API for both provisioning paths
- **State Tracking**: `DeploymentMode` field tracks provisioning type
- **Migration Support**: Can upgrade from shared to dedicated (and vice versa)

**Security & RBAC:**
- **Namespace Isolation**: Network policies enforce tenant separation
- **RBAC Controls**: Management API has limited cluster permissions
- **Secret Management**: Per-tenant credentials isolated
- **Audit Logging**: All provisioning actions logged
