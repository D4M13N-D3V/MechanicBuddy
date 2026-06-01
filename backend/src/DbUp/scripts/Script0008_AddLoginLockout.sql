-- =============================================================================
-- Add login lockout tracking to the user table
-- =============================================================================
-- Tracks consecutive failed login attempts and a lockout expiry so the
-- application can lock an account after too many failures (brute-force defence).

ALTER TABLE public."user" ADD COLUMN IF NOT EXISTS failed_login_attempts INTEGER NOT NULL DEFAULT 0;
ALTER TABLE public."user" ADD COLUMN IF NOT EXISTS locked_until TIMESTAMPTZ NULL;
