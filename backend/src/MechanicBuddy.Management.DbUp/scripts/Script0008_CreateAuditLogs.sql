-- =============================================================================
-- Create Audit Logs table for Management API
-- =============================================================================
-- This migration creates the audit_logs table to track all administrative
-- actions in the management portal for troubleshooting.

CREATE TABLE management.audit_logs (
    id SERIAL PRIMARY KEY,

    -- Who made the request
    admin_id INT REFERENCES management.super_admins(id) ON DELETE SET NULL,
    admin_email VARCHAR(255) NOT NULL,
    admin_role VARCHAR(50),
    ip_address VARCHAR(50),
    user_agent TEXT,

    -- What action was performed
    action_type VARCHAR(50) NOT NULL,  -- api_request, tenant_operation, auth, admin
    http_method VARCHAR(10) NOT NULL,  -- GET, POST, PUT, DELETE, PATCH
    endpoint VARCHAR(500) NOT NULL,
    resource_type VARCHAR(100),        -- tenant, demo_request, super_admin, etc.
    resource_id VARCHAR(100),          -- ID of affected resource if applicable
    tenant_id VARCHAR(50),             -- If operation affects a specific tenant
    action_description VARCHAR(500),   -- Human-readable description

    -- When it happened
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    duration_ms INT,                   -- Request duration in milliseconds

    -- Result
    status_code INT NOT NULL,
    was_successful BOOLEAN NOT NULL,

    -- Constraints
    CONSTRAINT valid_mgmt_action_type CHECK (action_type IN ('api_request', 'tenant_operation', 'auth', 'admin'))
);

-- Indexes for common query patterns
CREATE INDEX idx_mgmt_audit_logs_timestamp ON management.audit_logs(timestamp DESC);
CREATE INDEX idx_mgmt_audit_logs_admin_id ON management.audit_logs(admin_id);
CREATE INDEX idx_mgmt_audit_logs_action_type ON management.audit_logs(action_type);
CREATE INDEX idx_mgmt_audit_logs_tenant_id ON management.audit_logs(tenant_id);

-- Composite index for filtered queries
CREATE INDEX idx_mgmt_audit_logs_filter ON management.audit_logs(timestamp DESC, action_type, admin_email);
