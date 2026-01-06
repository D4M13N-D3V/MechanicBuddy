using Carmasters.Core.Domain;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Services
{
    public interface ITenantConfigRepository
    {
        Task<TenantRequisites> GetRequisitesAsync();
        Task<TenantPricing> GetPricingAsync();
        Task SaveRequisitesAsync(TenantRequisites requisites);
        Task SavePricingAsync(TenantPricing pricing);
    }
}