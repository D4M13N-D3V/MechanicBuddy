-- =============================================================================
-- Create Initial Super Admin
-- =============================================================================
-- Note: The password is pre-hashed using BCrypt.
-- Default password: admin123 (should be changed on first login)

INSERT INTO management.super_admins (
    email,
    password_hash,
    name,
    role,
    is_active
) VALUES (
    'admin@mechanicbuddy.app',
    -- BCrypt hash of 'admin123' - CHANGE THIS PASSWORD AFTER FIRST LOGIN
    '$2a$11$rKkPO6qoNhWp0tLbLx.hS.0qYXpFqnAMfzJLEpSOvVBnR8YcFJ8Hy',
    'System Administrator',
    'owner',
    true
) ON CONFLICT (email) DO NOTHING;
