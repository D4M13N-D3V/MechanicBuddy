-- Update the tier constraint to include new pricing tiers (solo, team, lifetime)
ALTER TABLE tenants DROP CONSTRAINT valid_tier;
ALTER TABLE tenants ADD CONSTRAINT valid_tier CHECK (tier IN ('demo', 'free', 'solo', 'team', 'lifetime', 'starter', 'professional', 'enterprise'));
