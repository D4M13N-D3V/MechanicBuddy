using Carmasters.Http.Api.Model;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Carmasters.Core.Application.Extensions.DependencyInjection
{
	public static class AutoMapperExtensions 
    {
		public static IServiceCollection AddAutoMapperToApp(this IServiceCollection services)
		{
			SqlMapper.AddTypeHandler(new Carmasters.Core.Application.Dapper.JsonNodeTypeHandler());
			services.AddAutoMapper(x => {
				x.AddProfile<DefaultProfile>();
			});
			return services;
		}
	}
}
