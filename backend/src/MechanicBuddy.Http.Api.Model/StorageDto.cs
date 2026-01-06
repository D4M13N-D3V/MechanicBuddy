using System;

namespace Carmasters.Http.Api.Models
{
    public record StorageDto(string Name, string Address, Guid Id, string Description, DateTime IntroducedAt) 
    {
        public StorageDto() : this(default, default, default, default, default) { }
    }

}