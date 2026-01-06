-- Script to manually create a super admin user
-- Run this after database migrations are complete

-- Password: 'Admin123!' (change this immediately after first login)
-- BCrypt hash generated with work factor 11

INSERT INTO super_admins (
    email,
    password_hash,
    name,
    role,
    is_active,
    created_at
) VALUES (
    'admin@mechanicbuddy.com',
    '$2a$11$XLKqKxH9XLqWqCvGKZJ7OuE3LNlJxKJGKp7JFmH8PK7LNlJxKJGKp7',  -- Change this!
    'System Administrator',
    'super_admin',
    true,
    NOW()
)
ON CONFLICT (email) DO NOTHING;

-- Verify the insert
SELECT id, email, name, role, is_active, created_at
FROM super_admins
WHERE email = 'admin@mechanicbuddy.com';
