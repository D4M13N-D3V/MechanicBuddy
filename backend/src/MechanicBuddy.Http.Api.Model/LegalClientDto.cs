using System;

namespace Carmasters.Http.Api.Models
{
    public record ClientDto(Guid Id, AddressDto Address, string Phone, string[] EmailAddresses, string CurrentEmail, bool SsAsshole, string Description, DateTime IntroducedAt,bool IsPrivate) 
    {
    }
    public record LegalClientDto(Guid Id,string Name,string RegNr, AddressDto Address, string Phone, string[] EmailAddresses, string CurrentEmail, bool IsAsshole, string Description, DateTime IntroducedAt) 
        : ClientDto(Id,Address,Phone,EmailAddresses,CurrentEmail, IsAsshole, Description,IntroducedAt,false)
    {
        public LegalClientDto() : this(default, default, default, default, default, default, default, default, default, default) { }
    }

    public record PrivateClientDto(Guid Id, string FirstName, string LastName, AddressDto Address, string Phone, string[] EmailAddresses, string CurrentEmail, bool IsAsshole, string Description,string PersonalCode, DateTime IntroducedAt)
      : ClientDto(Id, Address, Phone, EmailAddresses, CurrentEmail, IsAsshole, Description, IntroducedAt,true)
    {
        public PrivateClientDto() : this(default, default, default, default, default, default, default, default, default, default,default) { }
    }

    public record AddressDto(string Country=null, string Region = null, string City = null, string Street = null, string PostalCode = null) 
    { 
         
    }

    public record ModelError(string FieldName,string ErrorText)
    { 
    }
}