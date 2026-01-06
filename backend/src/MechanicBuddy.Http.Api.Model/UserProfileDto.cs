using System;

namespace Carmasters.Http.Api.Models
{
    public record UserProfileDto(string FirstName,string LastName,string Email,string UserName, string ProfileImageBase64)
    {
    }

    public record PasswordChangeDto(string CurrentPassword,string NewPassword,string ConfirmPassword) 
    {
    }
    /// <summary>
    /// Data transfer object for users retrieved from the tenancy database.
    /// Used to transfer user data between data layers without exposing domain entities.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// The username used for authentication
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The hashed password stored in the database
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The tenant name that identifies which database this user belongs to
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Whether the user account has been validated
        /// </summary>
        public bool Validated { get; set; }

        /// <summary>
        /// The employee ID associated with this user, if applicable
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Profile image stored as a byte array
        /// </summary>
        public byte[] ProfileImage { get; set; }
    }

}