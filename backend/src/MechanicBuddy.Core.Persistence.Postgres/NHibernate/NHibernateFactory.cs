using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Carmasters.Core.Persistence.Postgres.Repositories;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Cfg;

namespace Carmasters.Core.Persistence.Postgres.NHibernate
{
    public class NNhibernateFactory
    {   
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Use assemlby where mapping T is</typeparam>
        /// <param name="connectionString">null then multitenancy</param>
        /// <returns></returns>
        public static ISessionFactory BuildSessionFactory(IEnumerable<Assembly> mappingAssemblies,string connectionString = null)
        {
             
            Configuration cfg = new Configuration();
            cfg.DataBaseIntegration(i =>
            {
                i.Dialect<global::NHibernate.Dialect.PostgreSQL83Dialect>();
                if (connectionString == null)
                {
                    i.ConnectionProvider<MultiTenancyConnectionDriver>();
                }
                else
                {
                    
                    i.ConnectionString = connectionString;
                }
            });

            foreach (var assemly in mappingAssemblies)
            {
                cfg.AddMappingsFromAssembly(assemly);
            }
           ISessionFactory sessionFactory = cfg.BuildSessionFactory();
              
            return sessionFactory;
        } 

    }
     
}
