using Stripe;
using Stripe.Checkout;
using Stripe.BillingPortal;

namespace MechanicBuddy.Management.Api.Infrastructure;

public class StripeClient : IStripeClient
{
    private readonly ILogger<StripeClient> _logger;

    public StripeClient(IConfiguration configuration, ILogger<StripeClient> logger)
    {
        _logger = logger;
        var apiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe SecretKey not configured");
        StripeConfiguration.ApiKey = apiKey;
    }

    // Customer management
    public async Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string> metadata)
    {
        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = metadata
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe customer {CustomerId} for {Email}", customer.Id, email);

        return customer.Id;
    }

    // Subscription management
    public async Task<string> CreateSubscriptionAsync(string customerId, string priceId, int quantity = 1, Dictionary<string, string>? metadata = null)
    {
        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = priceId,
                    Quantity = quantity
                }
            },
            Metadata = metadata,
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription"
            },
            Expand = new List<string> { "latest_invoice.payment_intent" }
        };

        var service = new SubscriptionService();
        var subscription = await service.CreateAsync(options);

        _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId} with quantity {Quantity}",
            subscription.Id, customerId, quantity);

        return subscription.Id;
    }

    public async Task UpdateSubscriptionQuantityAsync(string subscriptionId, int quantity)
    {
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(subscriptionId, new SubscriptionGetOptions
        {
            Expand = new List<string> { "items" }
        });

        if (subscription.Items?.Data == null || !subscription.Items.Data.Any())
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} has no items");
        }

        var subscriptionItemId = subscription.Items.Data.First().Id;
        var itemService = new SubscriptionItemService();

        await itemService.UpdateAsync(subscriptionItemId, new SubscriptionItemUpdateOptions
        {
            Quantity = quantity,
            ProrationBehavior = "always_invoice"
        });

        _logger.LogInformation("Updated Stripe subscription {SubscriptionId} quantity to {Quantity}", subscriptionId, quantity);
    }

    public async Task<string> UpdateSubscriptionPriceAsync(string subscriptionId, string newPriceId, int quantity, bool prorate = true)
    {
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(subscriptionId, new SubscriptionGetOptions
        {
            Expand = new List<string> { "items" }
        });

        if (subscription.Items?.Data == null || !subscription.Items.Data.Any())
        {
            throw new InvalidOperationException($"Subscription {subscriptionId} has no items");
        }

        var oldItemId = subscription.Items.Data.First().Id;

        var updateOptions = new SubscriptionUpdateOptions
        {
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Id = oldItemId,
                    Deleted = true
                },
                new SubscriptionItemOptions
                {
                    Price = newPriceId,
                    Quantity = quantity
                }
            },
            ProrationBehavior = prorate ? "always_invoice" : "none"
        };

        var updatedSubscription = await service.UpdateAsync(subscriptionId, updateOptions);

        _logger.LogInformation("Updated Stripe subscription {SubscriptionId} to price {PriceId} with quantity {Quantity}",
            subscriptionId, newPriceId, quantity);

        return updatedSubscription.Id;
    }

    public async Task CancelSubscriptionAsync(string subscriptionId, bool immediately = false)
    {
        var service = new SubscriptionService();

        if (immediately)
        {
            await service.CancelAsync(subscriptionId);
            _logger.LogInformation("Cancelled Stripe subscription {SubscriptionId} immediately", subscriptionId);
        }
        else
        {
            await service.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            });
            _logger.LogInformation("Scheduled Stripe subscription {SubscriptionId} to cancel at period end", subscriptionId);
        }
    }

    public async Task<SubscriptionInfo> GetSubscriptionAsync(string subscriptionId)
    {
        var service = new SubscriptionService();
        var subscription = await service.GetAsync(subscriptionId, new SubscriptionGetOptions
        {
            Expand = new List<string> { "items" }
        });

        var priceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id;
        var quantity = subscription.Items?.Data?.FirstOrDefault()?.Quantity ?? 0;

        return new SubscriptionInfo(
            Id: subscription.Id,
            Status: subscription.Status,
            PriceId: priceId,
            Quantity: (int)quantity,
            CurrentPeriodStart: subscription.CurrentPeriodStart,
            CurrentPeriodEnd: subscription.CurrentPeriodEnd,
            CancelAt: subscription.CancelAt,
            CancelAtPeriodEnd: subscription.CancelAtPeriodEnd,
            Metadata: subscription.Metadata
        );
    }

    // Payment methods and checkout
    public async Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl)
    {
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created checkout session {SessionId} for customer {CustomerId}", session.Id, customerId);

        return session.Url ?? session.Id;
    }

    public async Task<string> CreateOneTimeCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null)
    {
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
            {
                new Stripe.Checkout.SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new Stripe.Checkout.SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created one-time checkout session {SessionId} for customer {CustomerId}", session.Id, customerId);

        return session.Url ?? session.Id;
    }

    public async Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl)
    {
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);

        _logger.LogInformation("Created billing portal session {SessionId} for customer {CustomerId}", session.Id, customerId);

        return session.Url;
    }

    // Invoices and payment history
    public async Task<IEnumerable<InvoiceInfo>> GetCustomerInvoicesAsync(string customerId, int limit = 10)
    {
        var options = new InvoiceListOptions
        {
            Customer = customerId,
            Limit = limit
        };

        var service = new InvoiceService();
        var invoices = await service.ListAsync(options);

        return invoices.Data.Select(invoice => new InvoiceInfo(
            Id: invoice.Id,
            AmountDue: invoice.AmountDue / 100m,
            AmountPaid: invoice.AmountPaid / 100m,
            Currency: invoice.Currency?.ToUpperInvariant() ?? "USD",
            Status: invoice.Status ?? "unknown",
            PeriodStart: invoice.PeriodStart,
            PeriodEnd: invoice.PeriodEnd,
            Created: invoice.Created,
            HostedInvoiceUrl: invoice.HostedInvoiceUrl,
            InvoicePdf: invoice.InvoicePdf
        ));
    }

    public async Task<InvoiceInfo?> GetInvoiceAsync(string invoiceId)
    {
        var service = new InvoiceService();
        try
        {
            var invoice = await service.GetAsync(invoiceId);

            return new InvoiceInfo(
                Id: invoice.Id,
                AmountDue: invoice.AmountDue / 100m,
                AmountPaid: invoice.AmountPaid / 100m,
                Currency: invoice.Currency?.ToUpperInvariant() ?? "USD",
                Status: invoice.Status ?? "unknown",
                PeriodStart: invoice.PeriodStart,
                PeriodEnd: invoice.PeriodEnd,
                Created: invoice.Created,
                HostedInvoiceUrl: invoice.HostedInvoiceUrl,
                InvoicePdf: invoice.InvoicePdf
            );
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve invoice {InvoiceId}", invoiceId);
            return null;
        }
    }

    // Usage reporting (for metered billing)
    public async Task RecordUsageAsync(string subscriptionItemId, int quantity, DateTime timestamp, string? action = null)
    {
        var options = new UsageRecordCreateOptions
        {
            Quantity = quantity,
            Timestamp = timestamp,
            Action = action
        };

        var service = new UsageRecordService();
        await service.CreateAsync(subscriptionItemId, options);

        _logger.LogInformation("Recorded usage for subscription item {SubscriptionItemId}: {Quantity} at {Timestamp}",
            subscriptionItemId, quantity, timestamp);
    }
}
