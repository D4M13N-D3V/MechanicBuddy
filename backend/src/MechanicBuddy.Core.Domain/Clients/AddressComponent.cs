using System.Linq;

namespace Carmasters.Core.Domain
{
    public class AddressComponent
    {
        protected AddressComponent() { }
        public AddressComponent(string street,string country,string region,string city,string postalCode) 
        {
            Country = country;
            Region = region;
            Street = street;
            City = city;
            PostalCode = postalCode;
        }
        public virtual string Country { get; protected set; }
        public virtual string Region { get; protected set; }
        public virtual string City { get; protected set; }
        public virtual string Street { get; protected set; }
        public virtual string PostalCode { get; protected set; }

        public override string ToString()
        {
            return string.Join(", ", new string[] { Country, Region, City, Street, PostalCode }.Where(x=>!string.IsNullOrWhiteSpace(x)));
        }
    }
}