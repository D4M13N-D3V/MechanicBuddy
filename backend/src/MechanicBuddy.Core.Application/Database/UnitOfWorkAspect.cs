using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Database
{

    public class UnitOfWorkAspect : IAsyncActionFilter
    {
        private readonly ILogger<UnitOfWorkAspect> logger;
		private readonly IServiceProvider serviceProvider;

		public UnitOfWorkAspect(ILogger<UnitOfWorkAspect> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
			this.serviceProvider = serviceProvider;
		}

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.HttpContext.User.Identity.IsAuthenticated && //ISession is created only when user is authenticated
				 IsEditCall(context.HttpContext.Request.Method))
			{
				await RunWithinTransaction(next);
			}
			else await next();

		}

		private static bool IsEditCall(string httpMethod)
		{
			return httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "DELETE";
		}

		private async Task RunWithinTransaction(ActionExecutionDelegate next)
		{
			logger.LogDebug("Starting database transaction");
			var session = serviceProvider.GetRequiredService<ISession>();
			var transaction = session.BeginTransaction();
			try
			{
				var executed = await next();
				if (executed.Exception == null)
				{
					logger.LogDebug("No errors, commiting");
					transaction.Commit();
				}
				else
				{
					logger.LogDebug("Errors, rolling back any changes");
					transaction.Rollback();
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Errors, rolling back any changes");
				transaction.Rollback();
				throw;
			}
		}
	}
}
