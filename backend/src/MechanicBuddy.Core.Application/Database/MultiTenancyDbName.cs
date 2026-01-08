using MechanicBuddy.Core.Application.Configuration;
using System;
using System.Text.RegularExpressions;

namespace MechanicBuddy.Core.Application.Database
{
    public class MultiTenancyDbName
    {
        string value;
        private readonly DbOptions options;

        // Security: Regex to validate tenant names - only lowercase letters, numbers, and hyphens
        // Maximum 63 characters (PostgreSQL database name limit minus prefix)
        private static readonly Regex ValidTenantNamePattern = new Regex(
            @"^[a-z][a-z0-9\-]{0,62}$",
            RegexOptions.Compiled);

        private MultiTenancyDbName(DbOptions options)
        {
            this.options = options;
            if (string.IsNullOrWhiteSpace(options.Name))
            {
                throw new ArgumentException($"'{nameof(options.Name)}' cannot be null or whitespace.", nameof(options.Name));
            }
            EnsureTenacyEnabled();
        }

        public MultiTenancyDbName(DbOptions options, string tenantName) : this(options)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                throw new ArgumentException($"'{nameof(tenantName)}' cannot be null or whitespace.", nameof(tenantName));
            }

            // Security: Validate tenant name to prevent database injection
            var normalizedName = tenantName.ToLowerInvariant().Trim();
            if (!ValidTenantNamePattern.IsMatch(normalizedName))
            {
                throw new ArgumentException($"Invalid tenant name format. Must start with a letter, contain only lowercase letters, numbers, and hyphens, and be at most 63 characters.", nameof(tenantName));
            }

            value = $"{options.Name}-{normalizedName}";
        }
        public MultiTenancyDbName(DbOptions options, DbKind kind) : this(options)
        {
            if (options.MultiTenancy?.Suffix == null || options.MultiTenancy?.Suffix?.Tenancy == null || options.MultiTenancy?.Suffix?.Template == null)
            {
                throw new ArgumentException($"Multitenancy options not complete, check your configuration");
            }
            value = $"{options.Name}-{(kind == DbKind.Template ? options.MultiTenancy.Suffix.Template : options.MultiTenancy.Suffix.Tenancy)}";
        }

        public static implicit operator string(MultiTenancyDbName multiTenancyDb)
        {
            return multiTenancyDb.ToString();
        }
        private void EnsureTenacyEnabled()
        {
            if (!options.MultiTenancy?.Enabled == true) throw new Exception("MultiTenancy not enabled.");
        }
        public string Value => value;

        public override string ToString()
        {
            return value;
        }
    }
}
