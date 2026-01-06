using System;

namespace Carmasters.Http.Api.Models
{
    public record EmployeeDto  (string FirstName, string LastName, string Phone, string Email, string Proffession, string Description, DateTime IntroducedAt,string UserName,string Password, Guid Id) 
    {
        public EmployeeDto() : this(default, default, default, default, default,default,default,default,default,default) { }

        public string UserName { get; set; } = UserName;
    }
}