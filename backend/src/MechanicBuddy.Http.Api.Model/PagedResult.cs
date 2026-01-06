namespace Carmasters.Http.Api.Models
{
    public class PagedResult<T> {
        public bool HasMore { get; set; }
        public T[] Items {get;set;}
    }

}