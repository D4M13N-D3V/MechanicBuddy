-- =============================================================================
-- Create Initial Super Admin
-- =============================================================================
-- Note: The password is pre-hashed using BCrypt.

INSERT INTO management.super_admins (
    email,
    password_hash,
    name,
    role,
    is_active
) VALUES (
    'admin@mechanicbuddy.app',
    '$2b$11$bS.b0IYaS22s83x/zuV1..t3MiR10ayvw64P3UnWJ2pFiDcX01Fve',
    'System Administrator',
    'owner',
    true
) ON CONFLICT (email) DO NOTHING;
