-- =============================================================================
-- MechanicBuddy Management Database Schema
-- =============================================================================

-- Create schemas
CREATE SCHEMA IF NOT EXISTS management;

-- =============================================================================
-- Tenants Table
-- =============================================================================
CREATE TABLE management.tenants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
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
    primary_domain VARCHAR(255),
    custom_domains TEXT[],

    -- Secrets (encrypted references, not actual secrets)
    jwt_secret_ref VARCHAR(255),

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    trial_expires_at TIMESTAMP WITH TIME ZONE,
    last_billed_at TIMESTAMP WITH TIME ZONE,
    suspended_at TIMESTAMP WITH TIME ZONE,
    deleted_at TIMESTAMP WITH TIME ZONE,

    -- Constraints
    CONSTRAINT valid_tier CHECK (tier IN ('demo', 'free', 'professional', 'enterprise')),
    CONSTRAINT valid_status CHECK (status IN ('provisioning', 'active', 'suspended', 'deleted'))
);

CREATE INDEX idx_tenants_tenant_id ON management.tenants(tenant_id);
CREATE INDEX idx_tenants_status ON management.tenants(status);
CREATE INDEX idx_tenants_stripe_customer ON management.tenants(stripe_customer_id);
CREATE INDEX idx_tenants_trial_expires ON management.tenants(trial_expires_at) WHERE trial_expires_at IS NOT NULL;

-- =============================================================================
-- Demo Requests Table
-- =============================================================================
CREATE TABLE management.demo_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    company_name VARCHAR(255),
    phone VARCHAR(50),
    message TEXT,

    -- Status tracking
    status VARCHAR(20) DEFAULT 'pending',
    tenant_id UUID REFERENCES management.tenants(id),

    -- IP tracking for rate limiting
    ip_address INET,
    user_agent TEXT,

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE,
    converted_at TIMESTAMP WITH TIME ZONE,

    CONSTRAINT valid_demo_status CHECK (status IN ('pending', 'provisioning', 'active', 'expired', 'converted'))
);

CREATE INDEX idx_demo_requests_email ON management.demo_requests(email);
CREATE INDEX idx_demo_requests_status ON management.demo_requests(status);
CREATE INDEX idx_demo_requests_expires ON management.demo_requests(expires_at) WHERE expires_at IS NOT NULL;

-- =============================================================================
-- Domain Verifications Table
-- =============================================================================
CREATE TABLE management.domain_verifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES management.tenants(id) NOT NULL,
    domain VARCHAR(255) NOT NULL,
    verification_token VARCHAR(100) NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    verified_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,

    UNIQUE(tenant_id, domain),
    CONSTRAINT valid_domain_status CHECK (status IN ('pending', 'verified', 'failed', 'expired'))
);

CREATE INDEX idx_domain_verifications_domain ON management.domain_verifications(domain);

-- =============================================================================
-- Super Admins Table
-- =============================================================================
CREATE TABLE management.super_admins (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'support',

    -- Status
    is_active BOOLEAN DEFAULT true,
    email_verified BOOLEAN DEFAULT false,

    -- Security
    failed_login_attempts INT DEFAULT 0,
    locked_until TIMESTAMP WITH TIME ZONE,

    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login_at TIMESTAMP WITH TIME ZONE,

    CONSTRAINT valid_admin_role CHECK (role IN ('support', 'admin', 'owner'))
);

CREATE INDEX idx_super_admins_email ON management.super_admins(email);

-- =============================================================================
-- Super Admin Access Log (Audit Trail)
-- =============================================================================
CREATE TABLE management.super_admin_access_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    super_admin_id UUID REFERENCES management.super_admins(id) NOT NULL,
    tenant_id UUID REFERENCES management.tenants(id) NOT NULL,

    -- Access details
    accessed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    session_ended_at TIMESTAMP WITH TIME ZONE,
    ip_address INET,
    user_agent TEXT,
    reason TEXT,

    -- Actions performed during session
    actions_performed JSONB DEFAULT '[]'::jsonb
);

CREATE INDEX idx_access_log_admin ON management.super_admin_access_log(super_admin_id);
CREATE INDEX idx_access_log_tenant ON management.super_admin_access_log(tenant_id);
CREATE INDEX idx_access_log_accessed ON management.super_admin_access_log(accessed_at);

-- =============================================================================
-- Tenant Metrics Table
-- =============================================================================
CREATE TABLE management.tenant_metrics (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES management.tenants(id) NOT NULL,
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),

    -- Mechanic counts (for billing)
    active_mechanics INT NOT NULL DEFAULT 0,
    total_employees INT NOT NULL DEFAULT 0,

    -- Usage metrics
    work_orders_count INT NOT NULL DEFAULT 0,
    work_orders_this_month INT NOT NULL DEFAULT 0,
    invoices_count INT NOT NULL DEFAULT 0,
    invoices_this_month INT NOT NULL DEFAULT 0,
    vehicles_count INT NOT NULL DEFAULT 0,
    clients_count INT NOT NULL DEFAULT 0,
    spare_parts_count INT NOT NULL DEFAULT 0,

    -- Storage metrics
    database_size_mb DECIMAL(10, 2),

    -- Activity metrics
    last_activity_at TIMESTAMP WITH TIME ZONE,
    monthly_active_users INT DEFAULT 0
);

CREATE INDEX idx_tenant_metrics_tenant_recorded ON management.tenant_metrics(tenant_id, recorded_at DESC);

-- =============================================================================
-- Billing Events Table (for audit and debugging)
-- =============================================================================
CREATE TABLE management.billing_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES management.tenants(id),
    stripe_event_id VARCHAR(100) UNIQUE,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    error_message TEXT
);

CREATE INDEX idx_billing_events_tenant ON management.billing_events(tenant_id);
CREATE INDEX idx_billing_events_stripe ON management.billing_events(stripe_event_id);
CREATE INDEX idx_billing_events_type ON management.billing_events(event_type);

-- =============================================================================
-- Subscription History Table
-- =============================================================================
CREATE TABLE management.subscription_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES management.tenants(id) NOT NULL,

    -- Subscription details
    tier VARCHAR(20) NOT NULL,
    mechanic_count INT NOT NULL,
    monthly_amount DECIMAL(10, 2) NOT NULL,

    -- Period
    period_start TIMESTAMP WITH TIME ZONE NOT NULL,
    period_end TIMESTAMP WITH TIME ZONE NOT NULL,

    -- Stripe references
    stripe_invoice_id VARCHAR(100),
    stripe_subscription_id VARCHAR(100),

    -- Status
    status VARCHAR(20) DEFAULT 'pending',
    paid_at TIMESTAMP WITH TIME ZONE,

    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),

    CONSTRAINT valid_sub_status CHECK (status IN ('pending', 'paid', 'failed', 'refunded'))
);

CREATE INDEX idx_subscription_history_tenant ON management.subscription_history(tenant_id);
CREATE INDEX idx_subscription_history_period ON management.subscription_history(period_start, period_end);

-- =============================================================================
-- System Settings Table
-- =============================================================================
CREATE TABLE management.system_settings (
    key VARCHAR(100) PRIMARY KEY,
    value JSONB NOT NULL,
    description TEXT,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_by UUID REFERENCES management.super_admins(id)
);

-- Insert default settings
INSERT INTO management.system_settings (key, value, description) VALUES
('pricing_tiers', '{
    "free": {"mechanics": 1, "price_per_mechanic": 0},
    "standard": {"mechanics_min": 2, "mechanics_max": 10, "price_per_mechanic": 20},
    "growth": {"mechanics_min": 11, "mechanics_max": 20, "price_per_mechanic": 15},
    "scale": {"mechanics_min": 21, "price_per_mechanic": 10}
}'::jsonb, 'Pricing tier configuration'),
('demo_settings', '{
    "duration_days": 7,
    "max_mechanics": 3,
    "populate_sample_data": true
}'::jsonb, 'Demo/trial configuration'),
('maintenance_mode', '{"enabled": false, "message": null}'::jsonb, 'Maintenance mode settings');

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

-- Apply to super_admins
CREATE TRIGGER trigger_super_admins_updated_at
    BEFORE UPDATE ON management.super_admins
    FOR EACH ROW
    EXECUTE FUNCTION management.update_updated_at();

-- =============================================================================
-- Views
-- =============================================================================

-- Active tenants with latest metrics
CREATE VIEW management.v_tenant_overview AS
SELECT
    t.id,
    t.tenant_id,
    t.company_name,
    t.tier,
    t.status,
    t.owner_email,
    t.primary_domain,
    t.created_at,
    t.trial_expires_at,
    m.active_mechanics,
    m.work_orders_this_month,
    m.last_activity_at,
    m.recorded_at as metrics_recorded_at
FROM management.tenants t
LEFT JOIN LATERAL (
    SELECT * FROM management.tenant_metrics
    WHERE tenant_id = t.id
    ORDER BY recorded_at DESC
    LIMIT 1
) m ON true
WHERE t.deleted_at IS NULL;

-- Revenue summary
CREATE VIEW management.v_revenue_summary AS
SELECT
    date_trunc('month', period_start) as month,
    COUNT(DISTINCT tenant_id) as active_tenants,
    SUM(mechanic_count) as total_mechanics,
    SUM(monthly_amount) as total_revenue
FROM management.subscription_history
WHERE status = 'paid'
GROUP BY date_trunc('month', period_start)
ORDER BY month DESC;
