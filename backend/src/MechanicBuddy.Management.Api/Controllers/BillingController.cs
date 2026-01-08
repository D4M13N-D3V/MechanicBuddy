using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;
using Stripe;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly BillingService _billingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        BillingService billingService,
        IConfiguration configuration,
        ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("create-customer")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var customerId = await _billingService.CreateCustomerAsync(
                request.TenantId,
                request.Email,
                request.Name
            );

            return Ok(new { customerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create customer");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("create-subscription")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var subscriptionId = await _billingService.CreateSubscriptionAsync(
                request.TenantId,
                request.PriceId
            );

            return Ok(new { subscriptionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("cancel-subscription")]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
    {
        try
        {
            var success = await _billingService.CancelSubscriptionAsync(request.TenantId);
            if (!success)
            {
                return NotFound();
            }

            return Ok(new { message = "Subscription cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history/{tenantId}")]
    public async Task<IActionResult> GetHistory(string tenantId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var events = await _billingService.GetTenantBillingHistoryAsync(tenantId, skip, take);
        return Ok(events);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var revenue = await _billingService.GetTotalRevenueAsync(startDate, endDate);
        return Ok(new { revenue, startDate, endDate });
    }

    [HttpPost("portal-session")]
    public async Task<IActionResult> CreatePortalSession([FromBody] CreatePortalSessionRequest request)
    {
        try
        {
            var portalUrl = await _billingService.CreateBillingPortalSessionAsync(
                request.TenantId,
                request.ReturnUrl
            );

            return Ok(new { url = portalUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create billing portal session for tenant {TenantId}", request.TenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sync-mechanics")]
    public async Task<IActionResult> SyncMechanics([FromBody] SyncMechanicsRequest request)
    {
        try
        {
            var success = await _billingService.UpdateMechanicCountAsync(
                request.TenantId,
                request.MechanicCount
            );

            if (!success)
            {
                return BadRequest(new { message = "Failed to sync mechanic count. Tenant may not exist or subscription not configured." });
            }

            return Ok(new { message = "Mechanic count synced successfully", mechanicCount = request.MechanicCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync mechanics for tenant {TenantId}", request.TenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("create-customer-and-subscription")]
    public async Task<IActionResult> CreateCustomerAndSubscription([FromBody] CreateCustomerAndSubscriptionRequest request)
    {
        try
        {
            var (customerId, subscriptionId) = await _billingService.CreateCustomerAndSubscriptionAsync(
                request.TenantId,
                request.Email,
                request.Name,
                request.MechanicCount
            );

            return Ok(new { customerId, subscriptionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create customer and subscription");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("invoices/{tenantId}")]
    public async Task<IActionResult> GetInvoices(string tenantId)
    {
        try
        {
            var invoices = await _billingService.GetBillingHistoryAsync(tenantId);
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get invoices for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var transactions = await _billingService.GetAllTransactionsAsync(skip, take);
        var total = await _billingService.GetTransactionCountAsync();
        return Ok(new
        {
            items = transactions,
            total,
            page = (skip / take) + 1,
            pageSize = take,
            totalPages = (int)Math.Ceiling((double)total / take)
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _billingService.GetBillingStatsAsync();
        return Ok(stats);
    }

    [HttpPost("checkout/team")]
    public async Task<IActionResult> CreateTeamCheckout([FromBody] CreateCheckoutRequest request)
    {
        try
        {
            var checkoutUrl = await _billingService.CreateTeamCheckoutSessionAsync(
                request.TenantId,
                request.ReturnUrl
            );

            return Ok(new { url = checkoutUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Team checkout session for tenant {TenantId}", request.TenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("checkout/lifetime")]
    public async Task<IActionResult> CreateLifetimeCheckout([FromBody] CreateCheckoutRequest request)
    {
        try
        {
            var checkoutUrl = await _billingService.CreateLifetimeCheckoutSessionAsync(
                request.TenantId,
                request.ReturnUrl
            );

            return Ok(new { url = checkoutUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Lifetime checkout session for tenant {TenantId}", request.TenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("subscription/{tenantId}")]
    public async Task<IActionResult> GetSubscriptionStatus(string tenantId)
    {
        try
        {
            var status = await _billingService.GetSubscriptionStatusAsync(tenantId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subscription status for tenant {TenantId}", tenantId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecret"]
            );

            await _billingService.HandleWebhookAsync(stripeEvent);

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }
}

public record CreateCustomerRequest(string TenantId, string Email, string Name);
public record CreateSubscriptionRequest(string TenantId, string PriceId);
public record CancelSubscriptionRequest(string TenantId);
public record CreatePortalSessionRequest(string TenantId, string ReturnUrl);
public record SyncMechanicsRequest(string TenantId, int MechanicCount);
public record CreateCustomerAndSubscriptionRequest(string TenantId, string Email, string Name, int MechanicCount);
public record CreateCheckoutRequest(string TenantId, string ReturnUrl);
