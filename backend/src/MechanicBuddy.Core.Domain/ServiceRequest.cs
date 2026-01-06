using System;

namespace MechanicBuddy.Core.Domain
{
    public enum ServiceRequestStatus
    {
        New,
        Contacted,
        Scheduled,
        Completed,
        Cancelled
    }

    public class ServiceRequest : GuidIdentityEntity
    {
        protected ServiceRequest() { }

        public ServiceRequest(
            string customerName,
            string phone,
            string email,
            string vehicleInfo,
            string serviceType,
            string message,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            CustomerName = customerName ?? throw new ArgumentNullException(nameof(customerName));
            Phone = phone;
            Email = email;
            VehicleInfo = vehicleInfo;
            ServiceType = serviceType;
            Message = message;
            Status = ServiceRequestStatus.New;
            SubmittedAt = DateTime.UtcNow;
        }

        public virtual string CustomerName { get; protected set; }
        public virtual string Phone { get; protected set; }
        public virtual string Email { get; protected set; }
        public virtual string VehicleInfo { get; protected set; }
        public virtual string ServiceType { get; protected set; }
        public virtual string Message { get; protected set; }
        public virtual ServiceRequestStatus Status { get; protected set; }
        public virtual DateTime SubmittedAt { get; protected set; }
        public virtual string Notes { get; protected set; }

        public virtual void UpdateStatus(ServiceRequestStatus status)
        {
            Status = status;
        }

        public virtual void AddNotes(string notes)
        {
            Notes = notes;
        }
    }
}
