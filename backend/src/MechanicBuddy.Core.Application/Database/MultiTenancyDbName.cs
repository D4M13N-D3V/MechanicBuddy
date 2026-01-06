using Carmasters.Core.Application.Configuration;
using System;

namespace Carmasters.Core.Application.Database
{
    public class MultiTenancyDbName
    {
        string value;
        private readonly DbOptions options;

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
            value = $"{options.Name}-{tenantName}";
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
