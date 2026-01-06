using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MechanicBuddy.Management.Api.Services;

namespace MechanicBuddy.Management.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoRequestsController : ControllerBase
{
    private readonly DemoRequestService _demoRequestService;
    private readonly ILogger<DemoRequestsController> _logger;

    public DemoRequestsController(DemoRequestService demoRequestService, ILogger<DemoRequestsController> logger)
    {
        _demoRequestService = demoRequestService;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateDemoRequestRequest request)
    {
        try
        {
            var demoRequest = await _demoRequestService.CreateRequestAsync(
                request.Email,
                request.CompanyName,
                request.PhoneNumber
            );

            return CreatedAtAction(nameof(GetById), new { id = demoRequest.Id }, demoRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var demoRequest = await _demoRequestService.GetByIdAsync(id);
        if (demoRequest == null)
        {
            return NotFound();
        }
        return Ok(demoRequest);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var requests = await _demoRequestService.GetAllAsync(skip, take);
        return Ok(requests);
    }

    [HttpGet("pending")]
    [Authorize]
    public async Task<IActionResult> GetPending()
    {
        var requests = await _demoRequestService.GetPendingAsync();
        return Ok(requests);
    }

    [HttpPost("{id}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveDemoRequest? request)
    {
        try
        {
            var demoRequest = await _demoRequestService.ApproveRequestAsync(id, request?.Notes);
            return Ok(demoRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDemoRequest request)
    {
        try
        {
            var demoRequest = await _demoRequestService.RejectRequestAsync(id, request.Reason);
            return Ok(demoRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("cleanup-expired")]
    [Authorize]
    public async Task<IActionResult> CleanupExpired()
    {
        var count = await _demoRequestService.CleanupExpiredDemosAsync();
        return Ok(new { cleanedCount = count });
    }

    [HttpPost("send-reminders")]
    [Authorize]
    public async Task<IActionResult> SendReminders()
    {
        var count = await _demoRequestService.SendExpiringDemoRemindersAsync();
        return Ok(new { remindersSent = count });
    }

    [HttpPost("{id}/convert")]
    [Authorize]
    public async Task<IActionResult> ConvertToPaid(int id, [FromBody] ConvertDemoRequest request)
    {
        try
        {
            var demoRequest = await _demoRequestService.ConvertToPaidAsync(id, request.Tier);
            return Ok(demoRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("pending-count")]
    [Authorize]
    public async Task<IActionResult> GetPendingCount()
    {
        var count = await _demoRequestService.GetPendingCountAsync();
        return Ok(new { count });
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDemoStatusRequest request)
    {
        try
        {
            var demoRequest = await _demoRequestService.UpdateStatusAsync(id, request.Status);
            return Ok(demoRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateDemoRequestRequest(string Email, string CompanyName, string? PhoneNumber);
public record ApproveDemoRequest(string? Notes);
public record RejectDemoRequest(string Reason);
public record ConvertDemoRequest(string Tier);
public record UpdateDemoStatusRequest(string Status);
