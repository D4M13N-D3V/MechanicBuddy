using Carmasters.Core.Application;
using Carmasters.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace Carmasters.Core.Application.Extensions
{
    public static class ControllerExtensions
    {
        public static Guid? EmployeeId(this ControllerBase controller)
        {
            var empString = controller.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.UserData)?.Value;
            if (string.IsNullOrWhiteSpace(empString)) return null;
            return Guid.Parse(empString);

        }
        public static string TenantName(this ControllerBase controller)
        {
            return controller.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Spn)?.Value;
        }
        public static string UserName(this ControllerBase controller)
        {
            return controller.HttpContext.User.Identity.Name;
        }
    }
}
