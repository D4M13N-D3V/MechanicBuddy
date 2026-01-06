using MechanicBuddy.Management.Api.Domain;
using MechanicBuddy.Management.Api.Repositories;
using MechanicBuddy.Management.Api.Infrastructure;
using Stripe;

namespace MechanicBuddy.Management.Api.Services;

public class BillingService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBillingEventRepository _billingEventRepository;
    private readonly Infrastructure.IStripeClient _stripeClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingService> _logger;

    // Pricing tiers based on mechanic count
    private const int FREE_MAX_MECHANICS = 1;
    private const int STANDARD_MIN_MECHANICS = 2;
    private const int STANDARD_MAX_MECHANICS = 10;
    private const int GROWTH_MIN_MECHANICS = 11;
    private const int GROWTH_MAX_MECHANICS = 20;
    private const int SCALE_MIN_MECHANICS = 21;

    public BillingService(
        ITenantRepository tenantRepository,
        IBillingEventRepository billingEventRepository,
        Infrastructure.IStripeClient stripeClient,
        IConfiguration configuration,
        ILogger<BillingService> logger)
    {
        _tenantRepository = tenantRepository;
        _billingEventRepository = billingEventRepository;
        _stripeClient = stripeClient;
        _configuration = configuration;
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
                await HandleSubscriptionUpdateEventAsync(stripeEvent);
                break;

            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedEventAsync(stripeEvent);
                break;

            case "invoice.payment_succeeded":
                await HandlePaymentSucceededEventAsync(stripeEvent);
                break;

            case "invoice.payment_failed":
                await HandlePaymentFailedEventAsync(stripeEvent);
                break;

            default:
                _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    private async Task HandleSubscriptionUpdateEventAsync(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        await HandleSubscriptionUpdatedAsync(subscription);
    }

    private async Task HandleSubscriptionDeletedEventAsync(Event stripeEvent)
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

    private async Task HandlePaymentSucceededEventAsync(Event stripeEvent)
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

    private async Task HandlePaymentFailedEventAsync(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        await HandlePaymentFailedAsync(invoice);
    }

    public async Task<IEnumerable<BillingEvent>> GetTenantBillingHistoryAsync(string tenantId, int skip = 0, int take = 50)
    {
        return await _billingEventRepository.GetByTenantIdAsync(tenantId, skip, take);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _billingEventRepository.GetTotalRevenueAsync(startDate, endDate);
    }

    /// <summary>
    /// Creates a customer and subscription in one operation for new paid tenants
    /// </summary>
    public async Task<(string CustomerId, string SubscriptionId)> CreateCustomerAndSubscriptionAsync(
        string tenantId,
        string email,
        string name,
        int mechanicCount)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant not found");
        }

        // Create customer if doesn't exist
        string customerId;
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            customerId = await _stripeClient.CreateCustomerAsync(email, name, new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId
            });

            tenant.StripeCustomerId = customerId;
            await _tenantRepository.UpdateAsync(tenant);
        }
        else
        {
            customerId = tenant.StripeCustomerId;
        }

        // Calculate appropriate price ID based on mechanic count
        var priceId = CalculatePriceForMechanicCount(mechanicCount);

        // Free tier doesn't need subscription
        if (priceId == null)
        {
            tenant.Tier = "free";
            tenant.MaxMechanics = FREE_MAX_MECHANICS;
            await _tenantRepository.UpdateAsync(tenant);
            return (customerId, string.Empty);
        }

        // Create subscription with metadata
        var subscriptionId = await _stripeClient.CreateSubscriptionAsync(
            customerId,
            priceId,
            mechanicCount,
            new Dictionary<string, string>
            {
                ["tenant_id"] = tenantId
            });

        tenant.StripeSubscriptionId = subscriptionId;
        tenant.MaxMechanics = mechanicCount;
        tenant.Tier = GetTierName(mechanicCount);
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "customer_and_subscription_created",
            Amount = 0,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["customer_id"] = customerId,
                ["subscription_id"] = subscriptionId,
                ["price_id"] = priceId,
                ["mechanic_count"] = mechanicCount
            }
        });

        _logger.LogInformation("Created customer {CustomerId} and subscription {SubscriptionId} for tenant {TenantId} with {MechanicCount} mechanics",
            customerId, subscriptionId, tenantId, mechanicCount);

        return (customerId, subscriptionId);
    }

    /// <summary>
    /// Updates the mechanic count for a tenant's subscription
    /// </summary>
    public async Task<bool> UpdateMechanicCountAsync(string tenantId, int mechanicCount)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found for mechanic count update", tenantId);
            return false;
        }

        // Free tier - no subscription needed
        if (mechanicCount <= FREE_MAX_MECHANICS)
        {
            tenant.MaxMechanics = mechanicCount;
            tenant.Tier = "free";
            await _tenantRepository.UpdateAsync(tenant);
            return true;
        }

        // If no subscription exists, tenant needs to create one first
        if (string.IsNullOrEmpty(tenant.StripeSubscriptionId))
        {
            _logger.LogWarning("Tenant {TenantId} has no subscription but needs one for {MechanicCount} mechanics",
                tenantId, mechanicCount);
            return false;
        }

        // Get current subscription to check price tier
        var currentSubscription = await _stripeClient.GetSubscriptionAsync(tenant.StripeSubscriptionId);
        var newPriceId = CalculatePriceForMechanicCount(mechanicCount);

        if (newPriceId == null)
        {
            _logger.LogError("Could not calculate price for {MechanicCount} mechanics", mechanicCount);
            return false;
        }

        // Check if we need to change price tier
        if (currentSubscription.PriceId != newPriceId)
        {
            // Price tier changed, update subscription price and quantity
            await _stripeClient.UpdateSubscriptionPriceAsync(
                tenant.StripeSubscriptionId,
                newPriceId,
                mechanicCount,
                prorate: true);

            _logger.LogInformation("Updated subscription {SubscriptionId} to new price tier {PriceId} with quantity {MechanicCount}",
                tenant.StripeSubscriptionId, newPriceId, mechanicCount);
        }
        else
        {
            // Same price tier, just update quantity
            await _stripeClient.UpdateSubscriptionQuantityAsync(tenant.StripeSubscriptionId, mechanicCount);

            _logger.LogInformation("Updated subscription {SubscriptionId} quantity to {MechanicCount}",
                tenant.StripeSubscriptionId, mechanicCount);
        }

        // Update tenant
        tenant.MaxMechanics = mechanicCount;
        tenant.Tier = GetTierName(mechanicCount);
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "mechanic_count_updated",
            Amount = 0,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["mechanic_count"] = mechanicCount,
                ["price_id"] = newPriceId
            }
        });

        return true;
    }

    /// <summary>
    /// Calculates the appropriate Stripe price ID based on mechanic count
    /// </summary>
    public string? CalculatePriceForMechanicCount(int mechanicCount)
    {
        if (mechanicCount <= FREE_MAX_MECHANICS)
        {
            return null; // Free tier
        }
        else if (mechanicCount <= STANDARD_MAX_MECHANICS)
        {
            return _configuration["Stripe:PriceIds:Standard"]
                ?? throw new InvalidOperationException("Standard price ID not configured");
        }
        else if (mechanicCount <= GROWTH_MAX_MECHANICS)
        {
            return _configuration["Stripe:PriceIds:Growth"]
                ?? throw new InvalidOperationException("Growth price ID not configured");
        }
        else
        {
            return _configuration["Stripe:PriceIds:Scale"]
                ?? throw new InvalidOperationException("Scale price ID not configured");
        }
    }

    /// <summary>
    /// Gets the tier name based on mechanic count
    /// </summary>
    private string GetTierName(int mechanicCount)
    {
        if (mechanicCount <= FREE_MAX_MECHANICS)
            return "free";
        else if (mechanicCount <= STANDARD_MAX_MECHANICS)
            return "standard";
        else if (mechanicCount <= GROWTH_MAX_MECHANICS)
            return "growth";
        else
            return "scale";
    }

    /// <summary>
    /// Creates a Stripe billing portal session for customer self-service
    /// </summary>
    public async Task<string> CreateBillingPortalSessionAsync(string tenantId, string returnUrl)
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

        var portalUrl = await _stripeClient.CreateBillingPortalSessionAsync(tenant.StripeCustomerId, returnUrl);

        _logger.LogInformation("Created billing portal session for tenant {TenantId}", tenantId);

        return portalUrl;
    }

    /// <summary>
    /// Gets billing history with invoice details for a tenant
    /// </summary>
    public async Task<IEnumerable<InvoiceInfo>> GetBillingHistoryAsync(string tenantId)
    {
        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null || string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            return Enumerable.Empty<InvoiceInfo>();
        }

        return await _stripeClient.GetCustomerInvoicesAsync(tenant.StripeCustomerId, limit: 50);
    }

    /// <summary>
    /// Handles subscription updated/created webhook events
    /// </summary>
    public async Task HandleSubscriptionUpdatedAsync(Subscription subscription)
    {
        var tenantId = subscription.Metadata.GetValueOrDefault("tenant_id");
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Subscription {SubscriptionId} has no tenant_id in metadata", subscription.Id);
            return;
        }

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found for subscription update", tenantId);
            return;
        }

        // Update subscription details
        tenant.StripeSubscriptionId = subscription.Id;
        tenant.Status = subscription.Status switch
        {
            "active" => "active",
            "past_due" => "suspended",
            "unpaid" => "suspended",
            "canceled" => "cancelled",
            "incomplete" => "suspended",
            "incomplete_expired" => "suspended",
            "trialing" => "active",
            _ => tenant.Status
        };

        if (subscription.CurrentPeriodEnd.HasValue)
        {
            tenant.SubscriptionEndsAt = subscription.CurrentPeriodEnd.Value;
        }

        // Update mechanic count from quantity
        if (subscription.Items?.Data != null && subscription.Items.Data.Any())
        {
            var quantity = subscription.Items.Data.First().Quantity ?? 0;
            tenant.MaxMechanics = (int)quantity;
            tenant.Tier = GetTierName((int)quantity);
        }

        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "subscription_updated",
            Amount = 0,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["subscription_id"] = subscription.Id,
                ["status"] = subscription.Status,
                ["quantity"] = subscription.Items?.Data?.FirstOrDefault()?.Quantity ?? 0
            }
        });

        _logger.LogInformation("Updated subscription {SubscriptionId} for tenant {TenantId} - Status: {Status}",
            subscription.Id, tenantId, subscription.Status);
    }

    /// <summary>
    /// Handles payment failed webhook events
    /// </summary>
    public async Task HandlePaymentFailedAsync(Invoice invoice)
    {
        // Try to get tenant ID from subscription metadata
        var tenantId = invoice.Subscription?.Metadata?.GetValueOrDefault("tenant_id");

        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("Invoice {InvoiceId} has no tenant_id in subscription metadata", invoice.Id);
            return;
        }

        var tenant = await _tenantRepository.GetByTenantIdAsync(tenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found for payment failure", tenantId);
            return;
        }

        // Suspend tenant on payment failure
        tenant.Status = "suspended";
        await _tenantRepository.UpdateAsync(tenant);

        await _billingEventRepository.CreateAsync(new BillingEvent
        {
            TenantId = tenantId,
            EventType = "payment_failed",
            Amount = (invoice.AmountDue ?? 0) / 100m,
            Currency = invoice.Currency?.ToUpperInvariant() ?? "USD",
            InvoiceId = invoice.Id,
            CreatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["attempt_count"] = invoice.AttemptCount ?? 0,
                ["next_payment_attempt"] = invoice.NextPaymentAttempt?.ToString("o") ?? "none"
            }
        });

        _logger.LogError("Payment failed for tenant {TenantId} - Invoice {InvoiceId}, Amount: {Amount}",
            tenantId, invoice.Id, invoice.AmountDue / 100m);
    }
}
