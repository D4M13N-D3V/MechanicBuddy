-- =============================================================================
-- MechanicBuddy Management Database Schema
-- =============================================================================

-- Create schemas
CREATE SCHEMA IF NOT EXISTS management;

-- =============================================================================
-- Tenants Table
-- =============================================================================
CREATE TABLE management.tenants (
    id SERIAL PRIMARY KEY,
    tenant_id VARCHAR(50) UNIQUE NOT NULL,
    company_name VARCHAR(255) NOT NULL,
    tier VARCHAR(20) NOT NULL DEFAULT 'free',
    status VARCHAR(20) NOT NULL DEFAULT 'provisioning',

    -- Contact information
    owner_email VARCHAR(255) NOT NULL,
    owner_name VARCHAR(255),
    phone VARCHAR(50),

    -- Billing (Stripe)
    stripe_customer_id VARCHAR(100),
    stripe_subscription_id VARCHAR(100),
    billing_email VARCHAR(255),

    -- Infrastructure
    k8s_namespace VARCHAR(100),
    db_connection_string TEXT,
    api_url VARCHAR(255),
    custom_domain VARCHAR(255),
    domain_verified BOOLEAN DEFAULT false,

    -- Limits
    max_mechanics INT DEFAULT 1,
    max_storage BIGINT DEFAULT 1073741824, -- 1GB in bytes

    -- Flags
    is_demo BOOLEAN DEFAULT false,

    -- Metadata
    metadata JSONB,

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    trial_ends_at TIMESTAMP WITH TIME ZONE,
    subscription_ends_at TIMESTAMP WITH TIME ZONE,
    suspended_at TIMESTAMP WITH TIME ZONE,
    deleted_at TIMESTAMP WITH TIME ZONE,

    -- Constraints
    CONSTRAINT valid_tier CHECK (tier IN ('demo', 'free', 'starter', 'professional', 'enterprise')),
    CONSTRAINT valid_status CHECK (status IN ('provisioning', 'active', 'suspended', 'deleted', 'trial'))
);

CREATE INDEX idx_tenants_tenant_id ON management.tenants(tenant_id);
CREATE INDEX idx_tenants_status ON management.tenants(status);
CREATE INDEX idx_tenants_stripe_customer ON management.tenants(stripe_customer_id);
CREATE INDEX idx_tenants_trial_expires ON management.tenants(trial_ends_at) WHERE trial_ends_at IS NOT NULL;

-- =============================================================================
-- Demo Requests Table
-- =============================================================================
CREATE TABLE management.demo_requests (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    company_name VARCHAR(255),
    phone_number VARCHAR(50),
    message TEXT,
    ip_address VARCHAR(50),

    -- Status tracking
    status VARCHAR(20) DEFAULT 'pending',
    tenant_id VARCHAR(50),

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    approved_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    expiring_soon_email_sent_at TIMESTAMP WITH TIME ZONE,

    -- Notes
    notes TEXT,
    rejection_reason TEXT,

    CONSTRAINT valid_demo_status CHECK (status IN ('pending', 'approved', 'provisioning', 'active', 'expired', 'converted', 'rejected'))
);

CREATE INDEX idx_demo_requests_email ON management.demo_requests(email);
CREATE INDEX idx_demo_requests_status ON management.demo_requests(status);
CREATE INDEX idx_demo_requests_expires ON management.demo_requests(expires_at) WHERE expires_at IS NOT NULL;

-- =============================================================================
-- Domain Verifications Table
-- =============================================================================
CREATE TABLE management.domain_verifications (
    id SERIAL PRIMARY KEY,
    tenant_id INT REFERENCES management.tenants(id) NOT NULL,
    domain VARCHAR(255) NOT NULL,
    verification_token VARCHAR(100) NOT NULL,
    verification_method VARCHAR(50) DEFAULT 'dns',
    is_verified BOOLEAN DEFAULT false,

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    verified_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,

    UNIQUE(tenant_id, domain)
);

CREATE INDEX idx_domain_verifications_domain ON management.domain_verifications(domain);

-- =============================================================================
-- Super Admins Table
-- =============================================================================
CREATE TABLE management.super_admins (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'admin',

    -- Status
    is_active BOOLEAN DEFAULT true,

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login_at TIMESTAMP WITH TIME ZONE,

    CONSTRAINT valid_admin_role CHECK (role IN ('support', 'admin', 'owner'))
);

CREATE INDEX idx_super_admins_email ON management.super_admins(email);

-- =============================================================================
-- Super Admin Access Log (Audit Trail)
-- =============================================================================
CREATE TABLE management.super_admin_access_logs (
    id SERIAL PRIMARY KEY,
    super_admin_id INT REFERENCES management.super_admins(id) ON DELETE CASCADE NOT NULL,
    tenant_id VARCHAR(50) NOT NULL,

    -- Access details
    accessed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ip_address VARCHAR(50),
    user_agent TEXT,
    purpose VARCHAR(100) DEFAULT 'troubleshooting',
    notes TEXT
);

CREATE INDEX idx_super_admin_access_logs_admin ON management.super_admin_access_logs(super_admin_id);
CREATE INDEX idx_super_admin_access_logs_tenant ON management.super_admin_access_logs(tenant_id);
CREATE INDEX idx_super_admin_access_logs_accessed_at ON management.super_admin_access_logs(accessed_at DESC);

-- =============================================================================
-- Super Admin Access Tokens (One-time access)
-- =============================================================================
CREATE TABLE management.super_admin_access_tokens (
    id SERIAL PRIMARY KEY,
    token VARCHAR(255) NOT NULL UNIQUE,
    super_admin_id INT REFERENCES management.super_admins(id) ON DELETE CASCADE NOT NULL,
    tenant_id VARCHAR(50) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    consumed_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_super_admin_access_tokens_token ON management.super_admin_access_tokens(token);
CREATE INDEX idx_super_admin_access_tokens_expires ON management.super_admin_access_tokens(expires_at);

-- =============================================================================
-- Tenant Metrics Table
-- =============================================================================
CREATE TABLE management.tenant_metrics (
    id SERIAL PRIMARY KEY,
    tenant_id VARCHAR(50) NOT NULL,
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),

    -- Usage metrics
    active_mechanics INT DEFAULT 0,
    work_orders_count INT DEFAULT 0,
    clients_count INT DEFAULT 0,
    vehicles_count INT DEFAULT 0,
    storage_used BIGINT DEFAULT 0,
    api_calls_count INT DEFAULT 0
);

CREATE INDEX idx_tenant_metrics_tenant ON management.tenant_metrics(tenant_id);
CREATE INDEX idx_tenant_metrics_recorded ON management.tenant_metrics(recorded_at DESC);

-- =============================================================================
-- Billing Events Table
-- =============================================================================
CREATE TABLE management.billing_events (
    id SERIAL PRIMARY KEY,
    tenant_id VARCHAR(50),
    event_type VARCHAR(100) NOT NULL,
    amount DECIMAL(10, 2) DEFAULT 0,
    currency VARCHAR(3) DEFAULT 'USD',
    stripe_event_id VARCHAR(100),
    invoice_id VARCHAR(100),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    metadata JSONB
);

CREATE INDEX idx_billing_events_tenant ON management.billing_events(tenant_id);
CREATE INDEX idx_billing_events_stripe ON management.billing_events(stripe_event_id);
CREATE INDEX idx_billing_events_type ON management.billing_events(event_type);

-- =============================================================================
-- System Settings Table
-- =============================================================================
CREATE TABLE management.system_settings (
    key VARCHAR(100) PRIMARY KEY,
    value JSONB NOT NULL,
    description TEXT,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Insert default settings
INSERT INTO management.system_settings (key, value, description) VALUES
('pricing_tiers', '{
    "free": {"mechanics": 1, "price_per_mechanic": 0},
    "standard": {"mechanics_min": 2, "mechanics_max": 9, "price_per_mechanic": 20},
    "volume": {"mechanics_min": 10, "price_per_mechanic": 10}
}'::jsonb, 'Pricing tier configuration'),
('demo_settings', '{
    "duration_days": 7,
    "max_mechanics": 3,
    "populate_sample_data": true
}'::jsonb, 'Demo/trial configuration');

-- =============================================================================
-- Functions and Triggers
-- =============================================================================

-- Update timestamp trigger
CREATE OR REPLACE FUNCTION management.update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to tenants
CREATE TRIGGER trigger_tenants_updated_at
    BEFORE UPDATE ON management.tenants
    FOR EACH ROW
    EXECUTE FUNCTION management.update_updated_at();

-- Cleanup expired tokens function
CREATE OR REPLACE FUNCTION management.cleanup_expired_access_tokens()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM management.super_admin_access_tokens
    WHERE expires_at < NOW() - INTERVAL '1 hour'
       OR consumed_at IS NOT NULL;
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;
