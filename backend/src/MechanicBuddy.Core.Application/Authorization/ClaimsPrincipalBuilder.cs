using System.Collections.Generic;
using System.Security.Claims;
using NHibernate.Mapping;

namespace Carmasters.Core.Application.Authorization
{
    public class ClaimsPrincipalBuilder 
    {
        private static ClaimsPrincipal Build(string name, string fullName, string tenantName, string employeeId, bool publicUse)
        {
            var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, name),
            new Claim("FullName", fullName),
            new Claim(ClaimTypes.Spn, tenantName),
            new Claim(ClaimTypes.UserData, employeeId),
        };

            if (!publicUse)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Root"));
            }

            var identity = new ClaimsIdentity(claims, "Basic");

            var principal = new ClaimsPrincipal(identity);

            return principal;
        }

        public static ClaimsPrincipal Build(User user,string fullName,bool publicUse) => Build(user.UserName,fullName, user.Id.TenantName, user.Id.EmployeeId.ToString(), publicUse);
    }
} 