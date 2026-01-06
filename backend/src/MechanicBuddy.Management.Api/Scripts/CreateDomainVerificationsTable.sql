-- Create domain_verifications table for custom domain verification
-- This table tracks domain verification attempts and status

CREATE TABLE IF NOT EXISTS domain_verifications (
    id SERIAL PRIMARY KEY,
    tenant_id INTEGER NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    domain VARCHAR(255) NOT NULL,
    verification_token VARCHAR(64) NOT NULL,
    verification_method VARCHAR(20) NOT NULL DEFAULT 'dns', -- 'dns' or 'file'
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    expires_at TIMESTAMP NULL,
    CONSTRAINT unique_domain_per_tenant UNIQUE (tenant_id, domain)
);

-- Create index on domain for faster lookups
CREATE INDEX IF NOT EXISTS idx_domain_verifications_domain ON domain_verifications(domain);

-- Create index on tenant_id for faster lookups
CREATE INDEX IF NOT EXISTS idx_domain_verifications_tenant_id ON domain_verifications(tenant_id);

-- Create index on verification_token for faster lookups
CREATE INDEX IF NOT EXISTS idx_domain_verifications_token ON domain_verifications(verification_token);
