using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Carmasters.Core.Application.Errors
{
    public class InvalidModelStateJsonResponseFactory
    {
        public static IActionResult Handle(ActionContext actionContext)
        {
            var logger = actionContext.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidModelStateJsonResponseFactory>>();
            logger.LogError(JsonSerializer.Serialize(actionContext.ModelState, new JsonSerializerOptions { WriteIndented = true }));

            var modelError = actionContext.ModelState.Keys.SelectMany(k => actionContext.ModelState[k].Errors).FirstOrDefault();
            if (modelError != null)
            {
                var badResponse = new BadRequestObjectResult(new JsonErrorDto(modelError.ErrorMessage, modelError.Exception?.ToString()));
                return badResponse;
            }
            return new BadRequestObjectResult(new JsonErrorDto("Invalid model error exception occured, see logs",null));

        }
    }
}
