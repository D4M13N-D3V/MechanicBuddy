-- Super Admin Access Audit Logs
CREATE TABLE IF NOT EXISTS super_admin_access_logs (
    id SERIAL PRIMARY KEY,
    super_admin_id INTEGER NOT NULL REFERENCES super_admins(id) ON DELETE CASCADE,
    tenant_id VARCHAR(50) NOT NULL,
    accessed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    ip_address VARCHAR(50),
    user_agent TEXT,
    purpose VARCHAR(100) DEFAULT 'troubleshooting',
    notes TEXT
);

CREATE INDEX idx_access_logs_admin ON super_admin_access_logs(super_admin_id);
CREATE INDEX idx_access_logs_tenant ON super_admin_access_logs(tenant_id);
CREATE INDEX idx_access_logs_accessed_at ON super_admin_access_logs(accessed_at DESC);

-- One-time access tokens for direct tenant access
CREATE TABLE IF NOT EXISTS super_admin_access_tokens (
    id SERIAL PRIMARY KEY,
    token VARCHAR(255) NOT NULL UNIQUE,
    super_admin_id INTEGER NOT NULL REFERENCES super_admins(id) ON DELETE CASCADE,
    tenant_id VARCHAR(50) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    consumed_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_access_tokens_token ON super_admin_access_tokens(token);
CREATE INDEX idx_access_tokens_expires ON super_admin_access_tokens(expires_at);

-- Cleanup expired tokens (run periodically)
CREATE OR REPLACE FUNCTION cleanup_expired_access_tokens()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM super_admin_access_tokens
    WHERE expires_at < NOW() - INTERVAL '1 hour'
       OR consumed_at IS NOT NULL;
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;
