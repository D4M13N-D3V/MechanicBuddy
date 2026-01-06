using System.Net.Http;
using System.Threading.Tasks;

namespace MechanicBuddy.Core.Domain
{
    public interface IPricingSender
    {
        Task Send(Pricing pricing );
    }
}