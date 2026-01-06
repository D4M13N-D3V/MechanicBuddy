using Carmasters.Core.Application.Configuration;
using System.Threading.Tasks;

namespace Carmasters.Core.Application.Services
{
    public interface ITenantConfigService
    {
        Task<RequisitesOptions> GetRequisitesAsync();
        Task<PricingOptions> GetPricingAsync();
        Task<AppOptions> GetAppOptionsAsync();
        Task SaveRequisitesAsync(RequisitesOptions requisitesOptions);
        Task SavePricingAsync(PricingOptions pricingOptions);
        Task SaveAppOptionsAsync(AppOptions appOptions);
    }
}