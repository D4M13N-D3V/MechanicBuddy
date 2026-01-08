using System;

namespace MechanicBuddy.Core.Domain
{
    /// <summary>
    /// Represents an audit log entry tracking API requests and operations.
    /// </summary>
    public class AuditLog : GuidIdentityEntity
    {
        protected AuditLog() { }

        public AuditLog(
            string userName,
            Guid? employeeId,
            string ipAddress,
            string userAgent,
            string actionType,
            string httpMethod,
            string endpoint,
            string resourceType,
            string resourceId,
            string actionDescription,
            int statusCode,
            bool wasSuccessful,
            int? durationMs = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name is required", nameof(userName));
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type is required", nameof(actionType));
            if (string.IsNullOrWhiteSpace(httpMethod))
                throw new ArgumentException("HTTP method is required", nameof(httpMethod));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint is required", nameof(endpoint));

            UserName = userName;
            EmployeeId = employeeId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ActionType = actionType;
            HttpMethod = httpMethod;
            Endpoint = endpoint;
            ResourceType = resourceType;
            ResourceId = resourceId;
            ActionDescription = actionDescription;
            Timestamp = DateTime.UtcNow;
            StatusCode = statusCode;
            WasSuccessful = wasSuccessful;
            DurationMs = durationMs;
        }

        // Who made the request
        public virtual string UserName { get; protected set; }
        public virtual Guid? EmployeeId { get; protected set; }
        public virtual string IpAddress { get; protected set; }
        public virtual string UserAgent { get; protected set; }

        // What action was performed
        public virtual string ActionType { get; protected set; }
        public virtual string HttpMethod { get; protected set; }
        public virtual string Endpoint { get; protected set; }
        public virtual string ResourceType { get; protected set; }
        public virtual string ResourceId { get; protected set; }
        public virtual string ActionDescription { get; protected set; }

        // When it happened
        public virtual DateTime Timestamp { get; protected set; }
        public virtual int? DurationMs { get; protected set; }

        // Result
        public virtual int StatusCode { get; protected set; }
        public virtual bool WasSuccessful { get; protected set; }
    }
}
