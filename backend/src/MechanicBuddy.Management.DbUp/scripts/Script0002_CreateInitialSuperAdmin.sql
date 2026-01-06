-- =============================================================================
-- Create Initial Super Admin
-- =============================================================================
-- Note: The password will be set by the application on first run using
-- the SUPER_ADMIN_INITIAL_PASSWORD environment variable.
-- This just creates a placeholder that must be activated.

INSERT INTO management.super_admins (
    email,
    password_hash,
    name,
    role,
    is_active,
    email_verified
) VALUES (
    -- Email from environment: SUPER_ADMIN_EMAIL
    '${SUPER_ADMIN_EMAIL}',
    -- Password hash will be set by application
    '$PLACEHOLDER$',
    'System Administrator',
    'owner',
    true,
    true
) ON CONFLICT (email) DO NOTHING;
