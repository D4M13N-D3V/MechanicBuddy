using System;

namespace Carmasters.Http.Api.Models
{
    public record SparePartDto(string Code, string Name, decimal Price, decimal Quantity, short? Discount, Guid? StorageId, string StorageName, Guid Id, string Description, DateTime IntroducedAt) 
    {
        public SparePartDto() : this(default, default, default, default, default, default, default, default,default,default) { }
    }

}