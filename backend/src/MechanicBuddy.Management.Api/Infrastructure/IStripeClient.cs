namespace MechanicBuddy.Management.Api.Infrastructure;

public interface IStripeClient
{
    // Customer management
    Task<string> CreateCustomerAsync(string email, string name, Dictionary<string, string> metadata);

    // Subscription management
    Task<string> CreateSubscriptionAsync(string customerId, string priceId, int quantity = 1, Dictionary<string, string>? metadata = null);
    Task UpdateSubscriptionQuantityAsync(string subscriptionId, int quantity);
    Task<string> UpdateSubscriptionPriceAsync(string subscriptionId, string newPriceId, int quantity, bool prorate = true);
    Task CancelSubscriptionAsync(string subscriptionId, bool immediately = false);
    Task<SubscriptionInfo> GetSubscriptionAsync(string subscriptionId);

    // Payment methods and checkout
    Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl);
    Task<string> CreateOneTimeCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null);
    Task<string> CreateBillingPortalSessionAsync(string customerId, string returnUrl);

    // Invoices and payment history
    Task<IEnumerable<InvoiceInfo>> GetCustomerInvoicesAsync(string customerId, int limit = 10);
    Task<InvoiceInfo?> GetInvoiceAsync(string invoiceId);

    // Usage reporting (for metered billing)
    Task RecordUsageAsync(string subscriptionItemId, int quantity, DateTime timestamp, string? action = null);
}

public record SubscriptionInfo(
    string Id,
    string Status,
    string? PriceId,
    int Quantity,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    DateTime? CancelAt,
    bool CancelAtPeriodEnd,
    Dictionary<string, string>? Metadata
);

public record InvoiceInfo(
    string Id,
    decimal AmountDue,
    decimal AmountPaid,
    string Currency,
    string Status,
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    DateTime Created,
    string? HostedInvoiceUrl,
    string? InvoicePdf
);
