using System;

namespace MechanicBuddy.Http.Api.Models
{
    /// <summary>
    /// Data transfer object for user management operations
    /// </summary>
    public class UserManagementDto
    {
        public Guid EmployeeId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Proffession { get; set; }
        public bool Validated { get; set; }
        public bool IsDefaultAdmin { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime IntroducedAt { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new user
    /// </summary>
    public class CreateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Proffession { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool MustChangePassword { get; set; } = true;
    }

    /// <summary>
    /// Data transfer object for updating an existing user
    /// </summary>
    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Proffession { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Response DTO for checking if tenant can manage users
    /// </summary>
    public class CanManageUsersDto
    {
        public bool CanManageUsers { get; set; }
    }
}
