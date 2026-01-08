-- =============================================================================
-- Add fields to user table for user management feature
-- =============================================================================
-- This migration adds support for distinguishing the default admin account
-- and requiring password changes for new users.

-- Add fields to user table for user management feature
ALTER TABLE public."user" ADD COLUMN IF NOT EXISTS is_default_admin BOOLEAN DEFAULT FALSE;
ALTER TABLE public."user" ADD COLUMN IF NOT EXISTS must_change_password BOOLEAN DEFAULT TRUE;

-- Mark existing admin user as default admin
UPDATE public."user" SET is_default_admin = TRUE WHERE username = 'admin';

-- Existing users should not be forced to change password (only new default admins)
UPDATE public."user" SET must_change_password = FALSE;
