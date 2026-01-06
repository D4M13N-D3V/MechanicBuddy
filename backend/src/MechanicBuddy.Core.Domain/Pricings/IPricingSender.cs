using System.Net.Http;
using System.Threading.Tasks;

namespace Carmasters.Core.Domain
{
    public interface IPricingSender
    {
        Task Send(Pricing pricing );
    }
}