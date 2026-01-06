using System;
using System.ComponentModel.DataAnnotations;

namespace Carmasters.Http.Api.Models
{ 
    public record ClientPageDto (bool IsCompany, string Name, DateTime IntroducedAt, string Address, string Phone, string Email, Guid Id) 
    {
        public ClientPageDto() : this(default, default, default, default, default, default, default) { }
    }

}