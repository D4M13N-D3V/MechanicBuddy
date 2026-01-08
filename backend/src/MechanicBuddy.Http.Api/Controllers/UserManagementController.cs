using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NHibernate;
using MechanicBuddy.Core.Application;
using MechanicBuddy.Core.Application.Authorization;
using MechanicBuddy.Core.Application.Configuration;
using MechanicBuddy.Core.Application.Database;
using MechanicBuddy.Core.Application.Extensions;
using MechanicBuddy.Core.Application.Model;
using MechanicBuddy.Core.Application.RateLimiting;
using MechanicBuddy.Core.Application.Services;
using MechanicBuddy.Core.Domain;
using MechanicBuddy.Http.Api.Models;

namespace MechanicBuddy.Http.Api.Controllers
{
    [TenantRateLimit]
    [Authorize(Policy = "ServerSidePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IRepository repository;
        private readonly ISession session;
        private readonly DbOptions dbOptions;

        public UserManagementController(
            IUserRepository userRepository,
            IRepository repository,
            ISession session,
            IOptions<DbOptions> dbOptions)
        {
            this.userRepository = userRepository;
            this.repository = repository;
            this.session = session;
            this.dbOptions = dbOptions.Value;
        }

        /// <summary>
        /// Get all users for the current tenant
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<UserManagementDto>> GetAll()
        {
            var tenantName = this.TenantName();
            var users = userRepository.GetAllByTenant(tenantName).ToList();
            var userDtos = new List<UserManagementDto>();

            foreach (var user in users)
            {
                if (user.Id?.EmployeeId == null) continue;

                var employee = session.Get<Employee>(user.Id.EmployeeId);
                if (employee == null) continue;

                // Query additional user fields from database
                var isDefaultAdmin = false;
                var mustChangePassword = false;

                using (var connection = CreateConnection())
                {
                    var userFields = connection.QuerySingleOrDefault<UserFieldsDto>(
                        @"SELECT is_default_admin as IsDefaultAdmin,
                                 must_change_password as MustChangePassword
                          FROM public.user
                          WHERE tenantname = @TenantName AND employeeid = @EmployeeId",
                        new { TenantName = tenantName, EmployeeId = user.Id.EmployeeId });

                    if (userFields != null)
                    {
                        isDefaultAdmin = userFields.IsDefaultAdmin;
                        mustChangePassword = userFields.MustChangePassword;
                    }
                }

                userDtos.Add(new UserManagementDto
                {
                    EmployeeId = user.Id.EmployeeId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    FullName = employee.Name,
                    Phone = employee.Phone,
                    Proffession = employee.Proffession,
                    Validated = user.Validated,
                    IsDefaultAdmin = isDefaultAdmin,
                    MustChangePassword = mustChangePassword,
                    IntroducedAt = employee.IntroducedAt
                });
            }

            return Ok(userDtos);
        }

        /// <summary>
        /// Get paginated list of users (for Search component)
        /// </summary>
        [HttpGet("page")]
        public ActionResult<PagedResult<UserManagementDto>> GetPage(
            string searchText,
            string orderby,
            int limit,
            int offset,
            bool desc)
        {
            var tenantName = this.TenantName();

            using (var connection = CreateConnection())
            {
                // Build the query
                var orderByClause = string.IsNullOrEmpty(orderby) ? "e.introducedat" : $"e.{orderby}";
                var orderDirection = desc ? "DESC" : "ASC";

                var whereClause = string.IsNullOrEmpty(searchText)
                    ? ""
                    : @" AND (LOWER(CONCAT_WS(' ', e.firstname, e.lastname, e.email, e.phone, u.username))
                         LIKE LOWER(@SearchText))";

                var countQuery = $@"
                    SELECT COUNT(*)
                    FROM domain.employee e
                    INNER JOIN public.user u ON u.employeeid = e.id AND u.tenantname = @TenantName
                    WHERE 1=1 {whereClause}";

                var selectQuery = $@"
                    SELECT
                        e.id as EmployeeId,
                        u.username as UserName,
                        u.email as Email,
                        e.firstname as FirstName,
                        e.lastname as LastName,
                        CONCAT(e.firstname, ' ', e.lastname) as FullName,
                        e.phone as Phone,
                        e.proffession as Proffession,
                        u.validated as Validated,
                        COALESCE(u.is_default_admin, false) as IsDefaultAdmin,
                        COALESCE(u.must_change_password, false) as MustChangePassword,
                        e.introducedat as IntroducedAt
                    FROM domain.employee e
                    INNER JOIN public.user u ON u.employeeid = e.id AND u.tenantname = @TenantName
                    WHERE 1=1 {whereClause}
                    ORDER BY {orderByClause} {orderDirection}
                    LIMIT @Limit OFFSET @Offset";

                var parameters = new
                {
                    TenantName = tenantName,
                    SearchText = $"%{searchText}%",
                    Limit = limit,
                    Offset = offset
                };

                var total = connection.QuerySingle<int>(countQuery, parameters);
                var items = connection.Query<UserManagementDto>(selectQuery, parameters).ToArray();

                return Ok(new PagedResult<UserManagementDto>
                {
                    Items = items,
                    HasMore = (offset + limit) < total
                });
            }
        }

        /// <summary>
        /// Get a specific user by employee ID
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<UserManagementDto> GetById(Guid id)
        {
            var tenantName = this.TenantName();
            var user = userRepository.GetBy(new UserIdentifier(tenantName, id));

            if (user == null)
                return NotFound($"User with employee ID {id} not found");

            var employee = session.Get<Employee>(id);
            if (employee == null)
                return NotFound($"Employee with ID {id} not found");

            // Query additional user fields
            var isDefaultAdmin = false;
            var mustChangePassword = false;

            using (var connection = CreateConnection())
            {
                var userFields = connection.QuerySingleOrDefault<UserFieldsDto>(
                    @"SELECT is_default_admin as IsDefaultAdmin,
                             must_change_password as MustChangePassword
                      FROM public.user
                      WHERE tenantname = @TenantName AND employeeid = @EmployeeId",
                    new { TenantName = tenantName, EmployeeId = id });

                if (userFields != null)
                {
                    isDefaultAdmin = userFields.IsDefaultAdmin;
                    mustChangePassword = userFields.MustChangePassword;
                }
            }

            var dto = new UserManagementDto
            {
                EmployeeId = user.Id.EmployeeId,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = employee.Name,
                Phone = employee.Phone,
                Proffession = employee.Proffession,
                Validated = user.Validated,
                IsDefaultAdmin = isDefaultAdmin,
                MustChangePassword = mustChangePassword,
                IntroducedAt = employee.IntroducedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public ActionResult<Guid> CreateUser([FromBody] CreateUserDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
                throw new UserException("Username is required");

            if (string.IsNullOrWhiteSpace(model.Password))
                throw new UserException("Password is required");

            if (string.IsNullOrWhiteSpace(model.FirstName))
                throw new UserException("First name is required");

            var tenantName = this.TenantName();

            // Check if username already exists
            var existingUser = userRepository.GetBy(model.UserName);
            if (existingUser != null)
                throw new UserException($"Username '{model.UserName}' is already taken");

            // Create employee record first
            var employee = new Employee(
                model.FirstName,
                model.LastName,
                DateTime.UtcNow,
                model.Phone,
                model.Email,
                model.Proffession,
                model.Description);

            repository.Add(employee);
            session.Flush();

            // Create user record
            var hashedPassword = PasswordHasher.getHash(model.Password);
            var user = new User(
                model.UserName,
                hashedPassword,
                model.Email,
                false, // validated
                null, // profile image
                new UserIdentifier(tenantName, employee.Id));

            userRepository.Update(user);

            // Set must_change_password flag if needed
            if (model.MustChangePassword)
            {
                using (var connection = CreateConnection())
                {
                    connection.Execute(
                        @"UPDATE public.user
                          SET must_change_password = @MustChangePassword
                          WHERE tenantname = @TenantName AND employeeid = @EmployeeId",
                        new
                        {
                            MustChangePassword = true,
                            TenantName = tenantName,
                            EmployeeId = employee.Id
                        });
                }
            }

            return Ok(employee.Id);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public ActionResult UpdateUser(Guid id, [FromBody] UpdateUserDto model)
        {
            var tenantName = this.TenantName();
            var user = userRepository.GetBy(new UserIdentifier(tenantName, id));

            if (user == null)
                return NotFound($"User with employee ID {id} not found");

            var employee = session.Get<Employee>(id);
            if (employee == null)
                return NotFound($"Employee with ID {id} not found");

            // Update employee details
            employee.Change(
                model.FirstName,
                model.LastName,
                model.Phone,
                model.Email,
                model.Proffession,
                model.Description);

            session.Update(employee);

            // Update user details
            if (!string.IsNullOrWhiteSpace(model.UserName) && model.UserName != user.UserName)
            {
                var existingUser = userRepository.GetBy(model.UserName);
                if (existingUser != null)
                    throw new UserException($"Username '{model.UserName}' is already taken");

                user.ChangeUserName(model.UserName);
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                user.ChangeEmail(model.Email);
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var hashedPassword = PasswordHasher.getHash(model.Password);
                user.ChangePassword(hashedPassword);
            }

            userRepository.Update(user);

            return Ok(id);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete]
        public ActionResult Delete([FromBody] Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return BadRequest("No user IDs provided");

            var tenantName = this.TenantName();

            foreach (var id in ids)
            {
                // Check if this is the default admin
                using (var connection = CreateConnection())
                {
                    var isDefaultAdmin = connection.QuerySingleOrDefault<bool>(
                        @"SELECT COALESCE(is_default_admin, false)
                          FROM public.user
                          WHERE tenantname = @TenantName AND employeeid = @EmployeeId",
                        new { TenantName = tenantName, EmployeeId = id });

                    if (isDefaultAdmin)
                    {
                        throw new UserException("Cannot delete the default admin user");
                    }
                }

                // Delete user record
                using (var connection = CreateConnection())
                {
                    connection.Execute(
                        @"DELETE FROM public.user
                          WHERE tenantname = @TenantName AND employeeid = @EmployeeId",
                        new { TenantName = tenantName, EmployeeId = id });
                }

                // Delete employee record
                var employee = session.Get<Employee>(id);
                if (employee != null)
                {
                    repository.Delete(employee);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Check if the current tenant can manage users
        /// </summary>
        [HttpGet("canmanage")]
        public ActionResult<CanManageUsersDto> CanManageUsers()
        {
            // For now, return true - this will be wired up to check tenant tier/features later
            return Ok(new CanManageUsersDto { CanManageUsers = true });
        }

        /// <summary>
        /// Helper method to create a database connection
        /// </summary>
        private DbConnection CreateConnection()
        {
            var databaseName = dbOptions.MultiTenancy?.Enabled == true
                ? new MultiTenancyDbName(dbOptions, DbKind.Tenancy)
                : dbOptions.Name;

            var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = dbOptions.Host,
                Port = dbOptions.Port,
                Username = dbOptions.UserId,
                Password = dbOptions.Password,
                Database = databaseName
            };

            var connection = new Npgsql.NpgsqlConnection(connectionBuilder.ToString());
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Internal DTO for querying user fields
        /// </summary>
        private class UserFieldsDto
        {
            public bool IsDefaultAdmin { get; set; }
            public bool MustChangePassword { get; set; }
        }
    }
}
