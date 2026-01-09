-- Migration: Add last_activity_at column and unsuspend all tenants
-- Purpose: Support inactivity-based auto-suspension for free tier tenants

-- Step 1: Add last_activity_at column to tenants table
ALTER TABLE management.tenants
ADD COLUMN IF NOT EXISTS last_activity_at TIMESTAMPTZ;

-- Step 2: Backfill last_activity_at with created_at for existing tenants
UPDATE management.tenants
SET last_activity_at = created_at
WHERE last_activity_at IS NULL;

-- Step 3: Unsuspend all currently suspended tenants
UPDATE management.tenants
SET status = 'active'
WHERE status = 'suspended';

-- Step 4: Create index for efficient inactivity queries
CREATE INDEX IF NOT EXISTS idx_tenants_tier_status_activity
ON management.tenants(tier, status, last_activity_at);
