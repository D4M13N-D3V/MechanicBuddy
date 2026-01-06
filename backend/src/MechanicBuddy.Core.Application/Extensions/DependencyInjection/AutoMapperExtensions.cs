using MechanicBuddy.Http.Api.Model;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicBuddy.Core.Application.Extensions.DependencyInjection
{
	public static class AutoMapperExtensions 
    {
		public static IServiceCollection AddAutoMapperToApp(this IServiceCollection services)
		{
			SqlMapper.AddTypeHandler(new MechanicBuddy.Core.Application.Dapper.JsonNodeTypeHandler());
			services.AddAutoMapper(x => {
				x.AddProfile<DefaultProfile>();
			});
			return services;
		}
	}
}
