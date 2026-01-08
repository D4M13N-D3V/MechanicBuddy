using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Application.Errors
{

    public class JsonExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<JsonExceptionHandler> logger;
        private readonly IHostEnvironment environment;

        public JsonExceptionHandler(ILogger<JsonExceptionHandler> logger, IHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature =
                httpContext.Features.Get<IExceptionHandlerPathFeature>();

            var actualException = exceptionHandlerPathFeature?.Error ?? exception;
            logger.LogError(actualException, message: null);

            // Security: Only include full exception details in development environment
            var includeDetails = environment.IsDevelopment();
            var error = new JsonErrorDto(actualException, includeDetails);

            var json = JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;
        }
    }
}
