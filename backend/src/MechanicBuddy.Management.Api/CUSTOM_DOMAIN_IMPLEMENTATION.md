# Custom Domain Support Implementation - Phase 7

This document describes the implementation of custom domain support for the MechanicBuddy SaaS platform.

## Overview

The custom domain feature allows tenants to use their own domain names instead of the default subdomain (e.g., `mycompany.mechanicbuddy.com`). This involves:

1. DNS verification to prove domain ownership
2. Automatic Kubernetes Ingress updates
3. Automatic SSL/TLS certificate provisioning via cert-manager

## Architecture

### Components

1. **DomainService** (`Services/DomainService.cs`)
   - Handles domain verification logic
   - Integrates with Kubernetes to update Ingress resources
   - Performs DNS TXT record verification using DnsClient.NET

2. **DomainsController** (`Controllers/DomainsController.cs`)
   - RESTful API endpoints for domain management
   - Provides tenant-specific domain operations

3. **KubernetesClientService** (`Services/KubernetesClientService.cs`)
   - Extended with Ingress management methods
   - Updates Ingress resources with custom domains
   - Configures cert-manager annotations for automatic TLS

4. **Database**
   - `domain_verifications` table tracks verification attempts and status

## API Endpoints

All endpoints are under `/api/tenants/{tenantId}/domains` and require authentication.

### POST `/api/tenants/{tenantId}/domains`
Add a custom domain and start the verification process.

**Request Body:**
```json
{
  "domain": "mycompany.com",
  "verificationMethod": "dns"
}
```

**Response:**
```json
{
  "id": 1,
  "domain": "mycompany.com",
  "verificationToken": "abc123...",
  "verificationMethod": "dns",
  "expiresAt": "2026-01-13T12:00:00Z",
  "instructions": {
    "type": "DNS TXT Record",
    "host": "_mechanicbuddy-verify.mycompany.com",
    "value": "abc123...",
    "alternativeValue": "mechanicbuddy-verification=abc123...",
    "description": "Add a TXT record to your DNS..."
  }
}
```

### GET `/api/tenants/{tenantId}/domains`
List all domains for a tenant.

**Response:**
```json
{
  "domains": [
    {
      "id": 1,
      "domain": "mycompany.com",
      "isVerified": true,
      "verificationMethod": "dns",
      "createdAt": "2026-01-06T12:00:00Z",
      "verifiedAt": "2026-01-06T13:00:00Z",
      "expiresAt": null
    }
  ]
}
```

### POST `/api/tenants/{tenantId}/domains/{domain}/verify`
Trigger verification for a domain.

**Response:**
```json
{
  "message": "Domain verified successfully",
  "domain": "mycompany.com",
  "verifiedAt": "2026-01-06T13:00:00Z"
}
```

### GET `/api/tenants/{tenantId}/domains/{domain}/status`
Get the verification status of a domain.

**Response:**
```json
{
  "domain": "mycompany.com",
  "isVerified": false,
  "verificationMethod": "dns",
  "verificationToken": "abc123...",
  "createdAt": "2026-01-06T12:00:00Z",
  "verifiedAt": null,
  "expiresAt": "2026-01-13T12:00:00Z",
  "instructions": {
    "type": "DNS TXT Record",
    "host": "_mechanicbuddy-verify.mycompany.com",
    "value": "abc123..."
  }
}
```

### DELETE `/api/tenants/{tenantId}/domains/{domain}`
Remove a custom domain from a tenant.

**Response:** 204 No Content

## DNS Verification Process

### DNS Method (Recommended)

1. Tenant adds their domain via API
2. System generates a unique verification token
3. Tenant adds a TXT record to their DNS:
   - **Host:** `_mechanicbuddy-verify.{domain}`
   - **Value:** `{verificationToken}`
4. Tenant triggers verification via API
5. System queries DNS for the TXT record
6. If token matches, domain is verified
7. System updates Kubernetes Ingress automatically

### File Verification Method (Alternative)

1. Tenant adds their domain via API
2. System generates a unique verification token
3. Tenant uploads a file to their domain:
   - **Path:** `https://{domain}/.well-known/mechanicbuddy-verification.txt`
   - **Content:** `{verificationToken}`
4. Tenant triggers verification via API
5. System makes HTTP request to verify file exists with correct content
6. If verified, system updates Kubernetes Ingress

## Kubernetes Integration

### Automatic Ingress Updates

When a domain is verified, the system automatically:

1. Finds the tenant's Ingress resource in their namespace
2. Adds the custom domain to the Ingress rules
3. Updates TLS configuration for the new domain
4. Adds cert-manager annotations for automatic certificate provisioning

**Example Ingress Update:**

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: tenant-ingress
  namespace: mechanicbuddy-tenant123
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
    cert-manager.io/acme-challenge-type: http01
spec:
  rules:
  - host: tenant123.mechanicbuddy.com  # Default subdomain
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: mechanicbuddy-web
            port:
              number: 3000
  - host: mycompany.com  # Custom domain
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: mechanicbuddy-web
            port:
              number: 3000
  tls:
  - hosts:
    - tenant123.mechanicbuddy.com
    secretName: tenant123-mechanicbuddy-com-tls
  - hosts:
    - mycompany.com
    secretName: mycompany-com-tls
```

### Certificate Management

Certificates are automatically provisioned by cert-manager when the Ingress is updated. The system:

1. Adds cert-manager annotations to the Ingress
2. Creates TLS entries for each domain
3. cert-manager detects the changes and provisions certificates via Let's Encrypt
4. Certificates are automatically renewed before expiration

## Database Schema

```sql
CREATE TABLE domain_verifications (
    id SERIAL PRIMARY KEY,
    tenant_id INTEGER NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    domain VARCHAR(255) NOT NULL,
    verification_token VARCHAR(64) NOT NULL,
    verification_method VARCHAR(20) NOT NULL DEFAULT 'dns',
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    expires_at TIMESTAMP NULL,
    CONSTRAINT unique_domain_per_tenant UNIQUE (tenant_id, domain)
);
```

## Dependencies

- **DnsClient.NET** (v1.7.0): DNS resolution for TXT record verification
- **KubernetesClient** (v14.0.8): Kubernetes API client for Ingress management

## Configuration

### Required Settings

In `appsettings.json` or `appsettings.Secrets.json`:

```json
{
  "Provisioning": {
    "NamespacePrefix": "mechanicbuddy-",
    "ClusterIssuer": "letsencrypt-prod"
  }
}
```

### Kubernetes Requirements

1. **cert-manager** must be installed in the cluster
2. **ClusterIssuer** configured for Let's Encrypt (or other ACME provider)
3. Ingress controller with TLS support (e.g., NGINX, Traefik)

## Security Considerations

1. **Domain Ownership Verification**
   - DNS verification proves domain ownership
   - Tokens expire after 7 days
   - Each domain can only be verified by one tenant

2. **Unique Domains**
   - System checks if domain is already in use by another tenant
   - Database constraint prevents duplicate domains per tenant

3. **TLS/SSL**
   - All custom domains automatically get TLS certificates
   - Certificates managed by cert-manager
   - HTTP-01 ACME challenge for domain validation

## Error Handling

Common error scenarios:

1. **Domain already in use**: Returns 400 Bad Request
2. **DNS verification fails**: Returns 400 Bad Request with descriptive message
3. **Ingress not found**: Logs warning, verification still succeeds
4. **Kubernetes API errors**: Logged and returned as 500 Internal Server Error

## Usage Example

### Step 1: Add Domain

```bash
curl -X POST https://management.mechanicbuddy.com/api/tenants/1/domains \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "domain": "mycompany.com",
    "verificationMethod": "dns"
  }'
```

### Step 2: Add DNS Record

Add TXT record to DNS:
- Host: `_mechanicbuddy-verify.mycompany.com`
- Value: `{token from response}`

### Step 3: Verify Domain

```bash
curl -X POST https://management.mechanicbuddy.com/api/tenants/1/domains/mycompany.com/verify \
  -H "Authorization: Bearer {token}"
```

### Step 4: Check Status

```bash
curl -X GET https://management.mechanicbuddy.com/api/tenants/1/domains/mycompany.com/status \
  -H "Authorization: Bearer {token}"
```

## Testing

To test the implementation:

1. **Unit Tests**: Test DNS verification logic with mock DNS client
2. **Integration Tests**: Test with real Kubernetes cluster
3. **Manual Testing**: Use tools like `dig` to verify DNS records

```bash
# Verify TXT record
dig TXT _mechanicbuddy-verify.mycompany.com

# Check Ingress configuration
kubectl get ingress -n mechanicbuddy-tenant123 -o yaml
```

## Troubleshooting

### Domain verification fails

1. Check DNS propagation: `dig TXT _mechanicbuddy-verify.{domain}`
2. Verify token matches: Check API response vs DNS record
3. Check logs: `kubectl logs -n mechanicbuddy-management {pod-name}`

### Certificate not provisioned

1. Check cert-manager logs: `kubectl logs -n cert-manager {pod-name}`
2. Verify ClusterIssuer is configured: `kubectl get clusterissuer`
3. Check Certificate resource: `kubectl get certificate -n {namespace}`

### Ingress not updated

1. Check Kubernetes permissions for Management API
2. Verify namespace exists: `kubectl get namespace {namespace}`
3. Check Ingress exists: `kubectl get ingress -n {namespace}`

## Future Enhancements

1. **Wildcard domains**: Support for `*.mycompany.com`
2. **Multiple domains per tenant**: Allow tenants to have multiple custom domains
3. **DNS provider integration**: Direct DNS record creation via API
4. **Health checks**: Periodic verification of domain configuration
5. **Domain analytics**: Track usage and performance by domain
