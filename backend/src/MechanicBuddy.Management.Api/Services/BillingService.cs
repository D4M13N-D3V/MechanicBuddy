using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;
using Stripe;

namespace MechanicBuddy.Management.Api.Services;

public class BillingService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBillingEventRepository _billingEventRepository;
    private readonly IStripeClient _stripeClient;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        ITenantRepository tenantRepository,
        IBillingEventRepository billingEventRepository,
        IStripeClient stripeClient,
        ILogger<BillingService> logger)
    {
        _tenantRepository = tenantRepository;
        _billingEventRepository = billingEventRepository;
        _stripeClient = stripeClient;
        _logger = logger;
    }

    public async Task<string> CreateCustomerAsync(string tenantId, string email, string name)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            return tenant.StripeCustomerId;
        }

        var customerId = await _stripeClient.CreateCustomerAsync(email, name, new Dictionary<string, string>
        {
            ["tenant_id"] = tenantId
        });

        tenant.StripeCustomerId = customerId;
        await _tenantRepository.UpdateAsync(tenant);

        _logger.LogInformation("Created Stripe customer {CustomerId} for tenant {TenantId}", customerId, tenantId);

        return customerId;
    }

    public async Task<string> CreateSubscriptionAsync(string tenantId, string priceId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new InvalidOperationException("Tenant does not have a Stripe customer");
        }

        var subscriptionId = await _stripeClient.CreateSubscriptionAsync(tenant.StripeCustomerId, priceId);

        tenant.StripeSubscriptionId = subscriptionId;
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "subscription_created",
            Amount = 0,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["subscription_id"] = subscriptionId,
                ["price_id"] = priceId
            }
        });

        _logger.LogInformation("Created subscription {SubscriptionId} for tenant {TenantId}", subscriptionId, tenantId);

        return subscriptionId;
    }

    public async Task<bool> CancelSubscriptionAsync(string tenantId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null || string.IsNullOrEmpty(tenant.StripeSubscriptionId))
        {
            return false;
        }

        await _stripeClient.CancelSubscriptionAsync(tenant.StripeSubscriptionId);

        tenant.StripeSubscriptionId = null;
        tenant.SubscriptionEndsAt = DateTime.UtcNow;
        tenant.Status = "cancelled";
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "subscription_cancelled",
            Amount = 0,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Cancelled subscription for tenant {TenantId}", tenantId);

        return true;
    }

    public async Task HandleWebhookAsync(Event stripeEvent)
    {
        _logger.LogInformation("Processing Stripe webhook: {EventType}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpdateAsync(stripeEvent);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(stripeEvent);
                break;

            case "invoice.payment_succeeded":
                await HandlePaymentSucceededAsync(stripeEvent);
                break;

            case "invoice.payment_failed":
                await HandlePaymentFailedAsync(stripeEvent);
                break;

            default:
                _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleSubscriptionUpdateAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var tenantId = subscription.Metadata.GetValueOrDefault("tenant_id");
        if (string.IsNullOrEmpty(tenantId)) return;

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null) return;

        tenant.StripeSubscriptionId = subscription.Id;
        tenant.Status = subscription.Status == "active" ? "active" : "suspended";

        if (subscription.CurrentPeriodEnd.HasValue)
        {
            tenant.SubscriptionEndsAt = subscription.CurrentPeriodEnd.Value;
        }

        await _tenantRepository.UpdateAsync(tenant);
    }

    private async Task HandleSubscriptionDeletedAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var tenantId = subscription.Metadata.GetValueOrDefault("tenant_id");
        if (string.IsNullOrEmpty(tenantId)) return;

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null) return;

        tenant.Status = "cancelled";
        tenant.StripeSubscriptionId = null;
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "subscription_cancelled",
            Amount = 0,
            StripeEventId = stripeEvent.Id,
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task HandlePaymentSucceededAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var tenantId = invoice.Subscription?.Metadata?.GetValueOrDefault("tenant_id");
        if (string.IsNullOrEmpty(tenantId)) return;

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "payment_succeeded",
            Amount = (invoice.AmountPaid ?? 0) / 100m, // Convert from cents
            Currency = invoice.Currency?.ToUpperInvariant() ?? "USD",
            StripeEventId = stripeEvent.Id,
            InvoiceId = invoice.Id,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Payment succeeded for tenant {TenantId}: {Amount}", tenantId, invoice.AmountPaid);
    }

    private async Task HandlePaymentFailedAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var tenantId = invoice.Subscription?.Metadata?.GetValueOrDefault("tenant_id");
        if (string.IsNullOrEmpty(tenantId)) return;

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant != null)
        {
            tenant.Status = "suspended";
            await _tenantRepository.UpdateAsync(tenant);
        }

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "payment_failed",
            Amount = (invoice.AmountDue ?? 0) / 100m,
            Currency = invoice.Currency?.ToUpperInvariant() ?? "USD",
            StripeEventId = stripeEvent.Id,
            InvoiceId = invoice.Id,
            CreatedAt = DateTime.UtcNow
        });

        _logger.LogWarning("Payment failed for tenant {TenantId}", tenantId);
    }

    public async Task<IEnumerable<BillingEvent>> GetTenantBillingHistoryAsync(string tenantId, int skip = 0, int take = 50)
    {
        return await _billingEventRepository.GetByTenantIdAsync(tenantId, skip, take);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _billingEventRepository.GetTotalRevenueAsync(startDate, endDate);
    }
}
