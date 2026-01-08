# MechanicBuddy Security Remediation Tracker

**Created:** 2026-01-08
**Last Updated:** 2026-01-08
**Status:** In Progress

---

## Summary

| Severity | Total | Fixed | Remaining |
|----------|-------|-------|-----------|
| CRITICAL | 9 | 9 | 0 |
| HIGH | 12 | 12 | 0 |
| MEDIUM | 11 | 9 | 2 |
| LOW | 5 | 0 | 5 |
| **TOTAL** | **37** | **30** | **7** |

---

## CRITICAL Issues

### 1. SQL Injection via String Interpolation
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Http.Api/Controllers/WorkController.cs:296-354`
  - `backend/src/MechanicBuddy.Core.Application/Services/PageResultQuery.cs:92`
  - `backend/src/MechanicBuddy.Core.Application/Services/WildcardTokens.cs`
- **Fix:** Added `AllTokensSanitized()` method to escape SQL special characters, LIKE wildcards, and limit token length. Updated PageResultQuery and WorkController to use sanitized tokens with ESCAPE clause.
- **Commit:** 2026-01-08

### 2. Session Cookies Without `secure` Flag
- **Status:** [x] FIXED
- **File:** `frontend/src/_lib/server/session.ts:44-66`
- **Fix:** Added `isProduction` check and set `secure: isProduction` on all cookies. Cookies are now secure in production but allow HTTP in development.
- **Commit:** 2026-01-08

### 3. SERVER_SECRET Transmitted in Login Request
- **Status:** [x] FIXED
- **Files:**
  - `frontend/src/app/auth/login/authenticate.ts`
  - `frontend/src/_lib/server/query-api.ts`
  - `backend/src/MechanicBuddy.Http.Api/Controllers/UsersController.cs`
  - `backend/src/MechanicBuddy.Http.Api.Model/LoginDto.cs`
- **Fix:** Moved ServerSecret from request body to `X-Server-Secret` header. Added `customHeaders` support to httpPost. Backend now reads from header with constant-time comparison using `CryptographicOperations.FixedTimeEquals`.
- **Commit:** 2026-01-08

### 4. XSS via dangerouslySetInnerHTML
- **Status:** [x] FIXED
- **File:** `frontend/src/app/print/[...slug]/Print.tsx`
- **Fix:** Installed `isomorphic-dompurify` and added HTML sanitization with strict whitelist of allowed tags and attributes.
- **Commit:** 2026-01-08

### 5. Command Injection in Database Backup
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Persistence.Postgres/DatabaseBackup.cs`
  - `backend/src/MechanicBuddy.Core.Application/ShellCommand.cs`
- **Fix:** Added `RunWithArgs` method using ArgumentList for proper argument escaping. DatabaseBackup now uses this method and passes password via PGPASSWORD environment variable instead of command line.
- **Commit:** 2026-01-08

### 6. Hardcoded Default Password ("carcare")
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Authorization/PasswordHasher.cs` - Added `SecurePasswordGenerator` class
  - `backend/src/MechanicBuddy.Core.Application/Services/DemoSetupService.cs`
  - `backend/src/MechanicBuddy.Management.Api/Infrastructure/TenantDatabaseProvisioner.cs`
  - `backend/src/DbUp/scripts/Script0001_CreateDefaultAdmin.cs`
- **Fix:** Added `SecurePasswordGenerator` using `RandomNumberGenerator` for cryptographic randomness. DemoSetupService and TenantDatabaseProvisioner now generate secure random passwords. Script0001 sets `must_change_password=true` flag requiring password change on first login.
- **Commit:** 2026-01-08

### 7. Public JWT Accessible to JavaScript
- **Status:** [x] FIXED
- **Files:**
  - `frontend/src/_lib/server/session.ts` - Made JWT cookie httpOnly
  - `frontend/src/app/backend-api/[...path]/route.ts` - Updated proxy to read JWT from httpOnly cookie
  - `frontend/src/_lib/client/query-api.ts` - Removed JWT reading (proxy handles auth)
  - `frontend/src/app/api/profile-image/route.ts` - Created secure profile image endpoint
  - `frontend/src/app/home/layout.tsx` - Updated to use secure profile image route
- **Fix:** Changed JWT cookie from `httpOnly: false` to `httpOnly: true`. Client-side code no longer reads the JWT directly. Backend-api proxy route reads JWT from httpOnly cookie and adds Authorization header. Created server-side /api/profile-image route for secure profile picture access.
- **Commit:** 2026-01-08

### 8. JWT Issuer/Audience Validation Disabled
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Authorization/AppJwtToken.cs`
  - `backend/src/MechanicBuddy.Core.Application/Extensions/DependencyInjection/AuthorizationExtensions.cs`
  - `backend/src/MechanicBuddy.Core.Application/Configuration/Options.cs`
- **Fix:** Enabled ValidateIssuer and ValidateAudience. Added Issuer and Audience to JwtOptions with defaults. Updated token generation to include issuer/audience. Also fixed ClockSkew to allow 30 seconds tolerance.
- **Commit:** 2026-01-08

### 9. Multitenancy Isolation Bypass Risk
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Core.Persistence.Postgres/UserRepository.cs`
- **Fix:** Added explicit security check in `GetBy(string userName)` and `GetByEmail(string email)` methods. When multitenancy is enabled, the code now throws `InvalidOperationException` if `TenantId` is null or empty. This prevents any accidental cross-tenant data access. Added security comments documenting the pattern.
- **Commit:** 2026-01-08

---

## HIGH Issues

### 10. Password Hash Partially Logged
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Http.Api/Controllers/UsersController.cs:110`
- **Fix:** Removed password hash from log message, now logs "Invalid password" without any hash material.
- **Commit:** 2026-01-08

### 11. X-Forwarded-For Trusted Without Validation
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Core.Application/RateLimiting/StandardRateLimitStrategy.cs`
- **Fix:** Added trusted proxy validation. Now only trusts X-Forwarded-For when request comes from loopback or private network IPs. Added IP address format validation.
- **Commit:** 2026-01-08

### 12. Path Traversal in PDF Generation
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Http.Api/Controllers/WorkController.cs`
  - `backend/src/MechanicBuddy.Core.Application/Services/PdfGenerator.cs`
- **Fix:** Added `Path.GetFileName()` sanitization to prevent directory traversal attacks on PDF file operations.
- **Commit:** 2026-01-08

### 13. Authorization Headers Logged in Debug
- **Status:** [x] FIXED
- **Files:**
  - `frontend/src/_lib/server/query-api.ts`
  - `frontend/src/app/api/servicerequest/[id]/status/route.ts`
- **Fix:** Removed all console.log statements that exposed headers, cookies, and sensitive data. Added development-only logging for non-sensitive info.
- **Commit:** 2026-01-08

### 14. HTTPS Metadata Not Required (Management API)
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Management.Api/Program.cs:67`
- **Fix:** Changed `RequireHttpsMetadata = false` to `RequireHttpsMetadata = !builder.Environment.IsDevelopment()`. Now requires HTTPS metadata in production while allowing HTTP in development for easier local testing.
- **Commit:** 2026-01-08

### 15. Missing Enum.Parse Validation
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Http.Api/Controllers/WorkController.cs`
- **Fix:** Changed `Enum.Parse<WorkStatus>(status)` to use `Enum.TryParse<WorkStatus>(status, true, out var newWorkStatus)` with proper error handling. Returns `BadRequest` with descriptive message for invalid status values instead of throwing an unhandled exception.
- **Commit:** 2026-01-08

### 16. OrderBy Parameter Injection
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Core.Application/Services/PageResultQuery.cs`
- **Fix:** Added compiled `Regex` pattern to validate orderby parameter. Pattern allows: column names (letters, numbers, underscores), optional table prefix with dot notation, optional ASC/DESC suffix, comma-separated multiple columns, and format placeholder {0} for dynamic sort direction. Throws `ArgumentException` for invalid patterns.
- **Commit:** 2026-01-08

### 17. No File Size Validation on Uploads
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Validation/ImageValidator.cs` - Created new validator
  - `backend/src/MechanicBuddy.Http.Api/Controllers/BrandingController.cs` - Added validation to logo, hero, and gallery endpoints
  - `backend/src/MechanicBuddy.Http.Api/Controllers/ProfileController.cs` - Added validation to profile image upload
- **Fix:** Created `ImageValidator` class with file size limits (2MB for logos, 10MB for large images), MIME type whitelist validation, and magic byte verification to ensure uploaded files match their declared types. Applied to all image upload endpoints.
- **Commit:** 2026-01-08

### 18. DB Password in Shell Command Arguments
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Core.Persistence.Postgres/DatabaseBackup.cs`
- **Fix:** Fixed as part of CRITICAL #5. Password now passed via PGPASSWORD environment variable instead of command line arguments.
- **Commit:** 2026-01-08

### 19. JWT Tenant Claim Relied Upon for Isolation
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Database/MultiTenancyDbName.cs` - Added tenant name validation
  - `backend/src/MechanicBuddy.Core.Persistence.Postgres/NHibernate/MultiTenancyConnectionDriver.cs` - Improved error handling
- **Fix:** Added regex validation for tenant names to prevent injection attacks. Tenant names must start with a letter and contain only lowercase letters, numbers, and hyphens (max 63 chars). MultiTenancyConnectionDriver now throws `UnauthorizedAccessException` for invalid tenant identifiers.
- **Commit:** 2026-01-08

### 20. No CSP Header
- **Status:** [x] FIXED
- **File:** `frontend/next.config.ts`
- **Fix:** Added comprehensive security headers: Content-Security-Policy, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection, Referrer-Policy, and Permissions-Policy.
- **Commit:** 2026-01-08

### 21. 7-Day JWT Token Expiration
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Http.Api/appsettings.json` - Changed default timeout from 8 days to 8 hours
  - `backend/src/MechanicBuddy.Core.Application/Authorization/AppJwtToken.cs` - Added maximum timeout enforcement
- **Fix:** Reduced default session timeout from 8 days to 8 hours. Added code-level enforcement: maximum session timeout capped at 24 hours, default of 8 hours if not configured. This prevents misconfiguration while still allowing reasonable session lengths.
- **Commit:** 2026-01-08

---

## MEDIUM Issues

### 22. No CSRF Token Implementation
- **Status:** [ ] Not Started
- **Files:** Throughout auth routes
- **Fix:** Implement CSRF tokens for state-changing operations
- **PR:**

### 23. CSS Injection via Branding Colors
- **Status:** [x] FIXED
- **Files:**
  - `frontend/src/_lib/colorValidator.ts` - Created new color validation utility
  - `frontend/src/_components/ThemeProvider.tsx` - Added color validation to all theme providers
  - `frontend/src/_components/PortalThemeProvider.tsx` - Added color validation
  - `frontend/src/app/home/layout.tsx` - Added server-side color validation for inline styles
- **Fix:** Created `colorValidator.ts` with regex patterns for hex, rgb(), rgba(), hsl(), hsla(), and named colors. Added `isValidColor()` for checking and `escapeColorForScript()` for safe script injection. Applied validation in ThemeProvider, LandingThemeProvider, PortalThemeProvider, and layout.tsx to prevent CSS/XSS injection.
- **Commit:** 2026-01-08

### 24. ServerSecret Comparison (Timing Attack)
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Http.Api/Controllers/UsersController.cs:93`
- **Fix:** Implemented constant-time comparison using `CryptographicOperations.FixedTimeEquals` (fixed as part of CRITICAL #3).
- **Commit:** 2026-01-08

### 25. Missing Content-Security-Policy
- **Status:** [x] FIXED
- **File:** `frontend/next.config.ts`
- **Fix:** Fixed as part of HIGH #20. Added comprehensive CSP header with strict defaults.
- **Commit:** 2026-01-08

### 26. Admin Check Uses Username Instead of Flag
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Management.Api/Infrastructure/TenantDatabaseProvisioner.cs` - Changed to use `is_default_admin` flag
  - `backend/src/DbUp/scripts/Script0001_CreateDefaultAdmin.cs` - Added `is_default_admin = true` to INSERT
- **Fix:** Changed admin user identification from `username = 'admin'` to `is_default_admin = true` flag. This prevents bypasses where someone could create a user named 'admin' who isn't the actual admin. Updated both the disable users query and admin creation to use the flag consistently.
- **Commit:** 2026-01-08

### 27. No Password Complexity Requirements
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Authorization/PasswordHasher.cs`
  - `backend/src/MechanicBuddy.Http.Api/Controllers/UserManagementController.cs`
  - `backend/src/MechanicBuddy.Http.Api/Controllers/ProfileController.cs`
  - `backend/src/MechanicBuddy.Http.Api/Controllers/EmployeesController.cs`
- **Fix:** Added `PasswordValidator` class with rules: min 8 chars, uppercase, lowercase, digit, special char, and common password check. Applied to all password creation and change endpoints.
- **Commit:** 2026-01-08

### 28. ClockSkew = Zero (Too Strict)
- **Status:** [x] FIXED
- **Files:**
  - `backend/src/MechanicBuddy.Core.Application/Authorization/AppJwtToken.cs`
  - `backend/src/MechanicBuddy.Core.Application/Extensions/DependencyInjection/AuthorizationExtensions.cs`
- **Fix:** Fixed as part of CRITICAL #8. Changed ClockSkew from zero to 30 seconds to allow for distributed systems.
- **Commit:** 2026-01-08

### 29. Header Injection in Content-Disposition
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Http.Api/Controllers/PricingsController.cs`
- **Fix:** Added filename sanitization with `Path.GetFileName()` and proper URI encoding for Content-Disposition header.
- **Commit:** 2026-01-08

### 30. No Composite PK on User Table
- **Status:** [ ] Not Started
- **File:** `backend/src/DbUp/scripts/Script0000_createSchema.sql`
- **Fix:** Add migration to create proper constraints
- **PR:**

### 31. No Input Length Validation on Search
- **Status:** [x] FIXED
- **File:** `backend/src/MechanicBuddy.Core.Application/Services/PageResultQuery.cs`
- **Fix:** Added `MaxSearchLength` (500 chars) and `MaxSearchTokens` (10) constants. Search text is now truncated to max length in `FilterBy()`. Number of search tokens is limited in `GetWhereRestriction()` to prevent query complexity attacks and potential DoS.
- **Commit:** 2026-01-08

### 32. Debug console.log in Production Code
- **Status:** [x] FIXED
- **Files:**
  - `frontend/src/_lib/server/query-api.ts`
  - `frontend/src/app/api/servicerequest/[id]/status/route.ts`
  - `frontend/src/_lib/server/session.ts`
- **Fix:** Removed all debug console.log statements. Added development-only error logging where needed.
- **Commit:** 2026-01-08

---

## LOW Issues

### 33. No Account Lockout After Failed Attempts
- **Status:** [ ] Not Started
- **File:** `backend/src/MechanicBuddy.Http.Api/Controllers/UsersController.cs:90`
- **Fix:** Implement account lockout mechanism
- **PR:**

### 34. Default DB Credentials in Dev Config
- **Status:** [ ] Not Started
- **File:** `backend/src/MechanicBuddy.Management.Api/appsettings.Development.json`
- **Fix:** Use environment variables or user secrets
- **PR:**

### 35. No Password History Enforcement
- **Status:** [ ] Not Started
- **Files:** N/A
- **Fix:** Implement password history tracking
- **PR:**

### 36. No Multi-Factor Authentication
- **Status:** [ ] Not Started
- **Files:** N/A
- **Fix:** Implement TOTP-based MFA
- **PR:**

### 37. BCrypt Using Older `$2a$` Variant
- **Status:** [ ] Not Started
- **Files:** Multiple scripts
- **Fix:** Update to use `$2b$` variant
- **PR:**

---

## Changelog

### 2026-01-08 (Session 6)
- Fixed 3 additional MEDIUM priority issues (30 total):
  - **MEDIUM #23:** CSS Injection - Created `colorValidator.ts` with comprehensive color validation (hex, rgb, hsl, named colors). Applied to ThemeProvider, PortalThemeProvider, and server-side layout styles
  - **MEDIUM #26:** Admin Flag - Changed from username='admin' check to `is_default_admin` flag for proper admin identification
  - **MEDIUM #31:** Search Input Length - Added max length (500 chars) and max tokens (10) limits to prevent DoS
- **All CRITICAL, HIGH, and most MEDIUM issues resolved!**
- Remaining: 2 MEDIUM (CSRF tokens, composite PK) and 5 LOW priority items

### 2026-01-08 (Session 5)
- Fixed 3 additional HIGH priority issues (27 total):
  - **HIGH #17:** File Upload Validation - Created `ImageValidator` with size limits, MIME type whitelist, and magic byte verification
  - **HIGH #19:** Tenant Claim Isolation - Added regex validation for tenant names to prevent injection attacks through database name construction
  - **HIGH #21:** JWT Expiration - Reduced default from 8 days to 8 hours, added code-level cap at 24 hours maximum
- **All CRITICAL and HIGH issues now resolved!**

### 2026-01-08 (Session 4)
- Fixed 6 additional security issues (24 total):
  - **CRITICAL #6:** Hardcoded Password - Added `SecurePasswordGenerator` with cryptographic random generation, updated DemoSetupService and TenantDatabaseProvisioner
  - **CRITICAL #7:** Public JWT - Made JWT cookie httpOnly, created server-side proxy for API calls and secure profile image endpoint
  - **CRITICAL #9:** Multitenancy Isolation - Added mandatory TenantId check with exception when missing in multitenancy mode
  - **HIGH #14:** HTTPS Metadata - Now required in production, optional in development
  - **HIGH #15:** Enum.Parse - Changed to TryParse with proper error handling
  - **HIGH #16:** OrderBy Injection - Added regex validation for column names and sort directions
- **All CRITICAL issues now resolved!**

### 2026-01-08 (Session 3)
- Fixed 10 additional security issues (18 total):
  - **CRITICAL #5:** Command Injection - Added secure `RunWithArgs` method with ArgumentList and PGPASSWORD environment variable
  - **CRITICAL #8:** JWT Validation - Enabled issuer/audience validation with configurable values
  - **HIGH #11:** X-Forwarded-For - Added trusted proxy validation and IP format checking
  - **HIGH #12:** Path Traversal - Added `Path.GetFileName()` sanitization to PDF operations
  - **HIGH #18:** DB Password - Now passed via environment variable (part of #5)
  - **HIGH #20:** Security Headers - Added CSP, X-Frame-Options, X-Content-Type-Options, and more
  - **MEDIUM #25:** CSP - Comprehensive policy added (part of #20)
  - **MEDIUM #27:** Password Complexity - Added PasswordValidator with strong password requirements
  - **MEDIUM #28:** ClockSkew - Changed from zero to 30 seconds (part of #8)
  - **MEDIUM #29:** Header Injection - Added proper filename sanitization and encoding

### 2026-01-08 (Session 2)
- Fixed 8 security issues:
  - **CRITICAL #1:** SQL Injection - Added sanitization to WildcardTokens and PageResultQuery
  - **CRITICAL #2:** Session cookies - Added secure flag for production
  - **CRITICAL #3:** SERVER_SECRET - Moved to header with constant-time comparison
  - **CRITICAL #4:** XSS - Added DOMPurify sanitization to Print component
  - **HIGH #10:** Password hash logging - Removed from log output
  - **HIGH #13:** Debug logging - Removed sensitive data exposure
  - **MEDIUM #24:** Timing attack - Added FixedTimeEquals comparison
  - **MEDIUM #32:** Console.log cleanup - Removed debug statements

### 2026-01-08 (Session 1)
- Initial document created
- 37 security issues identified and documented

