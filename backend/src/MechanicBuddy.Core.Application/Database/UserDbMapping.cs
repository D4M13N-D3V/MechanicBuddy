using Carmasters.Core.Domain;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Database
{
    public class UserDbMapping : ClassMap<User>
    {
        public UserDbMapping()
        {
            Schema("public");
            Table("user");

            CompositeId(x => x.Id).Access.BackingField()
              .KeyProperty(x => x.TenantName, x => x.Access.BackingField().ColumnName("tenantname"))
              .KeyProperty(x => x.EmployeeId, x => x.Access.BackingField().ColumnName("employeeid"));

            Map(x => x.UserName).Column("username").Access.BackingField();

            Map(x => x.ProfileImage).Access.BackingField().Column("profile_image");
            Map(x => x.Password).Column("password").Access.BackingField();
            Map(x => x.Email).Column("email").Access.BackingField();
            Map(x => x.Validated).Column("validated").Access.BackingField();
        }
    }
}
