# Tenant Provisioning Architecture

## System Overview

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
│  │  - Helm values building                                    │    │
│  │  - Orchestration logic                                     │    │
│  │  - Status monitoring                                       │    │
│  └────────┬────────────────────────────┬──────────────────────┘    │
│           │                            │                            │
│  ┌────────▼────────────┐      ┌────────▼──────────────────────┐    │
│  │   HelmService       │      │  KubernetesClientService      │    │
│  │  - helm install     │      │  - Namespace operations       │    │
│  │  - helm upgrade     │      │  - Pod monitoring             │    │
│  │  - helm uninstall   │      │  - Secret management          │    │
│  │  - Process wrapper  │      │  - Ingress queries            │    │
│  └─────────────────────┘      └───────────────────────────────┘    │
│                                                                      │
└──────────────────────┬─────────────────────────┬───────────────────┘
                       │                         │
                       │ helm CLI                │ Kubernetes API
                       │                         │
┌──────────────────────▼─────────────────────────▼───────────────────┐
│                     Kubernetes Cluster                              │
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  Namespace: tenant-acme-auto                                 │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐ │  │
│  │  │  PostgreSQL Cluster (CloudNativePG)                    │ │  │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐             │ │  │
│  │  │  │Instance-1│  │Instance-2│  │Instance-3│  (HA)       │ │  │
│  │  │  └────┬─────┘  └────┬─────┘  └────┬─────┘             │ │  │
│  │  │       │             │             │                    │ │  │
│  │  │       └─────────────┴─────────────┘                    │ │  │
│  │  │                     │                                  │ │  │
│  │  │       ┌─────────────▼────────────┐                     │ │  │
│  │  │       │   PVC: 50Gi Storage      │                     │ │  │
│  │  │       └──────────────────────────┘                     │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐ │  │
│  │  │  API Deployment                                        │ │  │
│  │  │  ┌──────────┐  ┌──────────┐                           │ │  │
│  │  │  │ API Pod 1│  │ API Pod 2│  (2 replicas)             │ │  │
│  │  │  └────┬─────┘  └────┬─────┘                           │ │  │
│  │  │       │             │                                  │ │  │
│  │  │       └─────────────┴──────────────┐                   │ │  │
│  │  │                     │              │                   │ │  │
│  │  │       ┌─────────────▼─────┐  ┌────▼─────────────┐     │ │  │
│  │  │       │  Service (API)    │  │  ConfigMap       │     │ │  │
│  │  │       └───────────────────┘  └──────────────────┘     │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐ │  │
│  │  │  Web Deployment                                        │ │  │
│  │  │  ┌──────────┐  ┌──────────┐                           │ │  │
│  │  │  │ Web Pod 1│  │ Web Pod 2│  (2 replicas)             │ │  │
│  │  │  └────┬─────┘  └────┬─────┘                           │ │  │
│  │  │       │             │                                  │ │  │
│  │  │       └─────────────┴──────────────┐                   │ │  │
│  │  │                     │              │                   │ │  │
│  │  │       ┌─────────────▼─────┐  ┌────▼─────────────┐     │ │  │
│  │  │       │  Service (Web)    │  │  ConfigMap       │     │ │  │
│  │  │       └───────────────────┘  └──────────────────┘     │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐ │  │
│  │  │  Ingress (NGINX)                                       │ │  │
│  │  │  - Host: acme-auto.mechanicbuddy.app                  │ │  │
│  │  │  - TLS: Let's Encrypt Certificate                     │ │  │
│  │  │  - Routes:                                             │ │  │
│  │  │    / → Web Service                                     │ │  │
│  │  │    /api → API Service                                  │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  │                                                              │  │
│  │  ┌────────────────────────────────────────────────────────┐ │  │
│  │  │  Secrets                                               │ │  │
│  │  │  - postgres-app: DB credentials                        │ │  │
│  │  │  - jwt-secret: API authentication                      │ │  │
│  │  │  - smtp-credentials: Email configuration               │ │  │
│  │  └────────────────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

## Provisioning Sequence Diagram

```
Client          Controller       Provisioning      Helm          K8s
  │                 │             Service          Service       Client
  │                 │                │                │            │
  │─────POST────────>               │                │            │
  │ Provision       │                │                │            │
  │ Request         │                │                │            │
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
  │  credentials}   │                │                │            │
  │                 │                │                │            │
```

## Component Interaction

```
┌────────────────────────────────────────────────────────────────┐
│                   TenantProvisioningService                     │
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
│  │  Step 8-13: Finalization                                │  │
│  │  ┌──────────────┐  ┌─────────────┐  ┌────────────────┐  │  │
│  │  │   Set URLs   │→ │   Set       │→ │   Build        │  │  │
│  │  │   & Creds    │  │   Resources │  │   Result       │  │  │
│  │  └──────────────┘  └─────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

## Data Flow

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
┌─────────────────────────────────────────────────────────────┐
│  Kubernetes Cluster                                          │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Namespace: mechanicbuddy-system                       │ │
│  │                                                        │ │
│  │  ┌──────────────────────────────────────────────────┐ │ │
│  │  │  Management API Deployment                       │ │ │
│  │  │  ┌────────────────────────────────────────────┐  │ │ │
│  │  │  │  Pod: management-api                       │  │ │ │
│  │  │  │  - .NET 9 Runtime                          │  │ │ │
│  │  │  │  - Provisioning Services                   │  │ │ │
│  │  │  │  - Helm CLI installed                      │  │ │ │
│  │  │  │  - ServiceAccount: mechanicbuddy-mgmt      │  │ │ │
│  │  │  └────────────────────────────────────────────┘  │ │ │
│  │  └──────────────────────────────────────────────────┘ │ │
│  │                                                        │ │
│  │  ┌──────────────────────────────────────────────────┐ │ │
│  │  │  RBAC (ClusterRole + ClusterRoleBinding)        │ │ │
│  │  │  - namespaces: create, delete, get, list        │ │ │
│  │  │  - pods: get, list, watch                       │ │ │
│  │  │  - deployments: create, update, delete          │ │ │
│  │  │  - ingresses: create, update, delete            │ │ │
│  │  │  - postgresql.cnpg.io: all operations          │ │ │
│  │  └──────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Namespace: tenant-acme-auto-a1b2c3                    │ │
│  │  (Created dynamically per tenant)                      │ │
│  │                                                        │ │
│  │  [PostgreSQL] [API x2] [Web x2] [Ingress] [Secrets]   │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Namespace: tenant-other-shop-x9y8z7                   │ │
│  │  (Another tenant, completely isolated)                 │ │
│  │                                                        │ │
│  │  [PostgreSQL] [API x2] [Web x2] [Ingress] [Secrets]   │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

This architecture ensures:
- **Isolation**: Each tenant in separate namespace
- **Scalability**: Management API can provision many tenants
- **Security**: RBAC limits what can be created
- **Observability**: Full logging and monitoring
- **Reliability**: HA for enterprise tiers
