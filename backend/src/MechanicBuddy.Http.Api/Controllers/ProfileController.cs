using System;
using Carmasters.Core.Application.Authorization;
using Carmasters.Core.Application.Database;
using Carmasters.Core.Application.Extensions;
using Carmasters.Core.Application.Model;
using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Domain;
using Carmasters.Http.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Carmasters.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository repository;
        private readonly NHibernate.ISession session;

        public ProfileController(IUserRepository repository,NHibernate.ISession session)
        {
            this.repository = repository;
            this.session = session;
        }
         
        [HttpGet()]
        public IActionResult Get()
        {
            if (this.EmployeeId() == null) return NotFound();
            var employee = session.Get<Employee>(this.EmployeeId().GetValueOrDefault());
            if (employee == null) return NotFound();
            var user = repository.GetBy(new UserIdentifier(this.TenantName(), employee.Id));
            return Ok(new UserProfileDto(employee.FirstName, employee.LastName,user.Email, user.UserName, user.ProfileImage == null? null: Convert.ToBase64String(user.ProfileImage)));
        }

        [HttpPut]
        public IActionResult Put([FromBody] UserProfileDto profile)
        {
             
            if (this.EmployeeId() == null) return NotFound();
            var employee = session.Get<Employee>(this.EmployeeId().GetValueOrDefault());
            if (employee == null) return NotFound();
            var user = repository.GetBy(new UserIdentifier(this.TenantName(), employee.Id));

            if (profile.UserName != user.UserName)
            {
                if (string.IsNullOrWhiteSpace(user.UserName))
                {
                    throw new UserException($"Username cannot be empty.");
                }
                if (repository.GetBy(profile.UserName) != null)
                {
                    throw new UserException($"Username '{profile.UserName}' is taken.");
                }
                user.ChangeUserName(profile.UserName);
            }

            user.ChangeEmail(profile.Email);
             
            var profileImage  = Convert.FromBase64String(profile.ProfileImageBase64);
            user.ChangeProfileImage(profileImage);
            repository.Update(user);

            employee.ChangeName(profile.FirstName, profile.LastName);
            session.Update(employee);

            return Ok(); 
        }

        [HttpPut("changepassword")]
        public IActionResult ChangePassword([FromBody] PasswordChangeDto model)
        {
            if (this.EmployeeId() == null) return NotFound(); 
            var user = repository.GetBy(new UserIdentifier(this.TenantName(), this.EmployeeId().GetValueOrDefault()));

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                throw new UserException("New password cannot be empty.");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                throw new UserException("New password does not match with confirmed password");
            }

            if (user == null || !PasswordHasher.verifyHash(
               model.CurrentPassword, user.Password))
            {
                    throw new UserException("Current password does not match.");
            }

            user.ChangePassword(PasswordHasher.getHash(model.NewPassword));
            repository.Update(user);
             
            return Ok();

        }

        [HttpDelete()]
        public  IActionResult DeleteAccount()
        {
            return Ok();
        }
    }

}
