using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Database
{
    public interface IMultiTenancyConnectionDriver
    {
        DbConnection GetConnection(string connectionString);
        string BuildConnectionString();
    }
}
