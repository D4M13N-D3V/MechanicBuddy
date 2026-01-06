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
