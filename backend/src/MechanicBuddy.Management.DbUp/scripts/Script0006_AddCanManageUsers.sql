-- =============================================================================
-- Add can_manage_users column to tenants table
-- =============================================================================
-- This column controls whether a tenant can manage multiple users.
-- Only available for team, professional, enterprise, and lifetime tiers.

ALTER TABLE management.tenants ADD COLUMN IF NOT EXISTS can_manage_users BOOLEAN NOT NULL DEFAULT FALSE;

-- Update existing tenants based on tier
UPDATE management.tenants SET can_manage_users = TRUE WHERE tier IN ('team', 'lifetime', 'professional', 'enterprise');
