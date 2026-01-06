using Carmasters.Core.Application.RateLimiting;
using Carmasters.Core.Domain;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Linq;

namespace Carmasters.Http.Api.Controllers
{
    [Route("api/[controller]")]
    public class ServiceRequestController : ControllerBase
    {
        private readonly ISession session;

        public ServiceRequestController(ISession session)
        {
            this.session = session;
        }

        // Public endpoint - no auth required for customer submissions
        [HttpPost("submit")]
        [AllowAnonymous]
        public IActionResult Submit([FromBody] SubmitServiceRequestDto model)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                return BadRequest("Customer name is required");
            }

            if (string.IsNullOrWhiteSpace(model.Phone) && string.IsNullOrWhiteSpace(model.Email))
            {
                return BadRequest("Either phone or email is required");
            }

            var request = new ServiceRequest(
                model.CustomerName,
                model.Phone,
                model.Email,
                model.VehicleInfo,
                model.ServiceType,
                model.Message
            );

            session.Save(request);
            session.Flush();

            return Ok(new { success = true, message = "Your service request has been submitted. We will contact you soon!" });
        }

        // Authenticated endpoints for staff
        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpGet]
        public IActionResult GetAll(string status = null)
        {
            var sql = @"SELECT 
                id, customername, phone, email, vehicleinfo, servicetype, 
                message, status, submittedat, notes 
                FROM domain.servicerequest";

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " WHERE status = @status";
            }

            sql += " ORDER BY submittedat DESC";

            var requests = session.Connection.Query<ServiceRequestDto>(sql, new { status }).ToList();

            return Ok(requests);
        }

        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var request = session.Get<ServiceRequest>(id);
            if (request == null)
            {
                return NotFound();
            }

            return Ok(new ServiceRequestDto
            {
                Id = request.Id,
                CustomerName = request.CustomerName,
                Phone = request.Phone,
                Email = request.Email,
                VehicleInfo = request.VehicleInfo,
                ServiceType = request.ServiceType,
                Message = request.Message,
                Status = request.Status.ToString(),
                SubmittedAt = request.SubmittedAt,
                Notes = request.Notes
            });
        }

        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(Guid id, [FromBody] UpdateStatusDto model)
        {
            var request = session.Get<ServiceRequest>(id);
            if (request == null)
            {
                return NotFound();
            }

            if (Enum.TryParse<ServiceRequestStatus>(model.Status, true, out var status))
            {
                request.UpdateStatus(status);
                session.Update(request);
                session.Flush();
                return Ok();
            }

            return BadRequest("Invalid status");
        }

        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpPut("{id}/notes")]
        public IActionResult UpdateNotes(Guid id, [FromBody] string notes)
        {
            var request = session.Get<ServiceRequest>(id);
            if (request == null)
            {
                return NotFound();
            }

            request.AddNotes(notes);
            session.Update(request);
            session.Flush();
            return Ok();
        }

        [TenantRateLimit]
        [Authorize(Policy = "ServerSidePolicy")]
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var request = session.Get<ServiceRequest>(id);
            if (request == null)
            {
                return NotFound();
            }

            session.Delete(request);
            session.Flush();
            return Ok();
        }
    }

    public class SubmitServiceRequestDto
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string VehicleInfo { get; set; }
        public string ServiceType { get; set; }
        public string Message { get; set; }
    }

    public class ServiceRequestDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string VehicleInfo { get; set; }
        public string ServiceType { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
    }
}
