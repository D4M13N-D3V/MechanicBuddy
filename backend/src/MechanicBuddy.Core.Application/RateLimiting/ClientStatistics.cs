using System;

namespace MechanicBuddy.Core.Application.RateLimiting
{
    public class ClientStatistics
        {
            public DateTime LastSuccessfulResponseTime { get; set; }
            public int NumberOfRequestsCompletedSuccessfully { get; set; }
        } 
}
