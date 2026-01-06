using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Carmasters.Core.Application.Errors
{

    public class JsonExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<JsonExceptionHandler> logger;

        public JsonExceptionHandler(ILogger<JsonExceptionHandler> logger)
        {
            this.logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";
            var error = new JsonErrorDto(exception); ;

            var exceptionHandlerPathFeature =
                httpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                error = new JsonErrorDto(exceptionHandlerPathFeature?.Error);
                logger.LogError(exceptionHandlerPathFeature?.Error, message: null);
            }
            else
            {
                logger.LogError(exception, message: null);
            }
            var json = JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
            await httpContext.Response.WriteAsync(json);

            return true;
        }
    }
}
