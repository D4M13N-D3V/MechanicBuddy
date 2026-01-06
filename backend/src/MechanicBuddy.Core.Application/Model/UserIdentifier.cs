using System;

namespace Carmasters.Core.Application.Model
{
    public class UserIdentifier
    {
        public virtual string TenantName { get; set; }
        public virtual Guid EmployeeId { get; set; }
        protected UserIdentifier() { }
        public UserIdentifier(string tenantName, Guid employeeId)
        {
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                throw new ArgumentException($"'{nameof(tenantName)}' cannot be null or whitespace.", nameof(tenantName));
            }

            TenantName = tenantName;
            EmployeeId = employeeId;
        }

        public override bool Equals(object obj)
        {
            return obj is UserIdentifier identifier &&
                   string.Equals(TenantName, identifier.TenantName) &&
                  Equals(EmployeeId, identifier.EmployeeId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TenantName, TenantName);
        }
    }
}