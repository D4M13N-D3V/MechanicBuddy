using Carmasters.Core.Application.Database;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Extensions.Builder
{
	public static class NHIbernateAppBuilderExtension
	{
		public static IApplicationBuilder UseNHibernate(this IApplicationBuilder app)
		{
			NHibernate.Cfg.Environment.ObjectsFactory = new NHibernateObjectFactory(app.ApplicationServices);
			return app;
		}

	}
}
