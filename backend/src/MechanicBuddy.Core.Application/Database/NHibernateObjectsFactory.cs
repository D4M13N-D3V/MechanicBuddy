using NHibernate.Bytecode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Database
{
    public class NHibernateObjectFactory : IObjectsFactory
    {
        private readonly IServiceProvider serviceProvider;

        public NHibernateObjectFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public object CreateInstance(Type type, bool nonPublic)
        {
            object instance = serviceProvider.GetService(type);
            if (instance != null) { return instance; }

            return Activator.CreateInstance(type, nonPublic);
        }

        public object CreateInstance(Type type, params object[] ctorArgs)
        {
            object instance = serviceProvider.GetService(type);
            if (instance != null) { return instance; }

            return Activator.CreateInstance(type, ctorArgs);
        }

        public object CreateInstance(Type type)
        {
            object instance = serviceProvider.GetService(type);
            if (instance != null) { return instance; }

            return Activator.CreateInstance(type);
        }
    }
}
