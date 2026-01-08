-- =============================================================================
-- Create Audit Logs table for tenant API
-- =============================================================================
-- This migration creates the audit_logs table to track all API requests,
-- CRUD operations, auth events, and admin actions for troubleshooting.

CREATE TABLE domain.audit_logs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Who made the request
    user_name VARCHAR(255) NOT NULL,
    employee_id uuid NULL,
    ip_address VARCHAR(50),
    user_agent TEXT,

    -- What action was performed
    action_type VARCHAR(50) NOT NULL,  -- api_request, crud, auth, admin
    http_method VARCHAR(10) NOT NULL,  -- GET, POST, PUT, DELETE, PATCH
    endpoint VARCHAR(500) NOT NULL,
    resource_type VARCHAR(100),        -- client, vehicle, work, user, etc.
    resource_id VARCHAR(100),          -- ID of affected resource if applicable
    action_description VARCHAR(500),   -- Human-readable description

    -- When it happened
    timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    duration_ms INT,                   -- Request duration in milliseconds

    -- Result
    status_code INT NOT NULL,
    was_successful BOOLEAN NOT NULL,

    -- Constraints
    CONSTRAINT valid_action_type CHECK (action_type IN ('api_request', 'crud', 'auth', 'admin'))
);

-- Indexes for common query patterns
CREATE INDEX idx_audit_logs_timestamp ON domain.audit_logs(timestamp DESC);
CREATE INDEX idx_audit_logs_user_name ON domain.audit_logs(user_name);
CREATE INDEX idx_audit_logs_action_type ON domain.audit_logs(action_type);
CREATE INDEX idx_audit_logs_resource_type ON domain.audit_logs(resource_type);
CREATE INDEX idx_audit_logs_endpoint ON domain.audit_logs(endpoint);

-- Composite index for filtered queries
CREATE INDEX idx_audit_logs_filter ON domain.audit_logs(timestamp DESC, action_type, user_name);
