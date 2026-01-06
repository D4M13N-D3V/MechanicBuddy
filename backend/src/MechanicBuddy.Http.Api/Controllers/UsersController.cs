using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Carmasters.Core;
using Carmasters.Core.Application;
using Carmasters.Core.Application.Authorization;
using Carmasters.Core.Application.Configuration;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Application.Services;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NHibernate;
using NHibernate.Cfg;
using PuppeteerSharp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Carmasters.Http.Api.Controllers
{

    /*
     TODO

    AuthController
Hosts authenticate and profilepicture; limited per IP.

UserProfileController
All authenticated user operations; decorate the whole class with [TenantRateLimit].
     
     */

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserRepository repository;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<UsersController> logger;
        private readonly IConfiguration configuration;
        private readonly ISmtpClientFactory smtp;
        private readonly IOptions<RequisitesOptions> requisites;
        private readonly IOptions<JwtOptions> jwtOptions;
        private readonly DbOptions dbOptions; 

        public UsersController(IUserRepository repository,IServiceProvider serviceProvider,  IOptions<JwtOptions> jwtOptions, IOptions<DbOptions> dbOptions, ILogger<UsersController> logger, IConfiguration configuration, ISmtpClientFactory smtp, IOptions<RequisitesOptions> requisites)
        { 
            this.repository = repository;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
            this.smtp = smtp;
            this.requisites = requisites;
            this.jwtOptions = jwtOptions;
            this.dbOptions = dbOptions.Value;
        }


        [AllowAnonymous, LimitRequests(MaxRequests = 10, TimeWindow = 60)]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto model)
        {
            const int SecondsToWaitOnFailedLogonAttempt = 3;
            
            if (jwtOptions.Value.ConsumerSecret != model.ServerSecret )
            {
                await Task.Delay(TimeSpan.FromSeconds(SecondsToWaitOnFailedLogonAttempt)); // wait on failure
                return Unauthorized();
            }

            var user = repository.GetBy(model.Username);
              
            if (user == null || !PasswordHasher.verifyHash(
                model.Password, user.Password))
            { 
                logger.LogInformation("Authentication failure: {user} {message}", model.Username, "Wrong password or username");
                await Task.Delay(TimeSpan.FromSeconds(SecondsToWaitOnFailedLogonAttempt)); // wait on failure
                return Unauthorized();
            }

            var fullName = repository.GetFullName(model.Username);
            var internalUsePrincipal = ClaimsPrincipalBuilder.Build(user, fullName, false);
            var publicUsePrincipal = ClaimsPrincipalBuilder.Build(user, fullName, true); 

            return Ok(new
            {
                Jwt = AppJwtToken.Generate(jwtOptions.Value, internalUsePrincipal),
                PublicJwt = AppJwtToken.Generate(jwtOptions.Value, publicUsePrincipal),
                Timeout = (int)jwtOptions.Value.SessionTimeout.TotalSeconds
            }); 
        }

        [AllowAnonymous, LimitRequests(MaxRequests = 60, TimeWindow = 60)]
        [HttpGet("profilepicture/{jwt?}")] 
        public IActionResult GetProfilePicture(string jwt)
        {
            try
            {
                var jwtToken = AppJwtToken.LoadJwt(jwtOptions.Value, jwt);
                var tenantName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Spn).Value; 
                var empId = Guid.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.UserData)?.Value);
                var user = repository.GetBy(new UserIdentifier(tenantName, empId)); 
                if(user == null) return File(new byte[0], "image/jpeg");
                return File(user.ProfileImage, "image/jpeg");
            }
            catch (Exception ex) //need to check this if fails, right now it has crashed the app multiple times
            {
                logger.LogError(ex, "Cannot resolve user picture");
                return File(new byte[0], "image/jpeg");
            }
        }
        
          
        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")] 
        [HttpPost("extendsession")]
        public IActionResult ExtendSession()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    logger.LogWarning("Extending session failed, user not logged in");
                    return Unauthorized();
                }
                logger.LogInformation("Successfully extended user session for user {name}", User.Identity.Name);

                 
                var jwt = AppJwtToken.Generate(jwtOptions.Value, HttpContext.User);
                return Ok(jwt);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Extending session failed, invalid token");
                return Unauthorized("invalid token");
            }
        }

    }
}
