using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Carmasters.Core.Application.Errors
{
    public class JsonResponseExceptionFilter : IActionFilter
    {
        private readonly ILogger<JsonResponseExceptionFilter> logger;

        public JsonResponseExceptionFilter(ILogger<JsonResponseExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is not null)
            {
                var exception = context.Exception;
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;


                var errorJson = new JsonErrorDto(context.Exception);
                context.Result = new JsonResult(errorJson);


                logger.LogError(exception, message: null);
                context.ExceptionHandled = true;
            }
        }
    }
}
