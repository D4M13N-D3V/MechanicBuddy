namespace MechanicBuddy.Management.Api.Domain;

public class TenantMetrics
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public int ActiveMechanics { get; set; }
    public int WorkOrdersCount { get; set; }
    public int ClientsCount { get; set; }
    public int VehiclesCount { get; set; }
    public long StorageUsed { get; set; } // bytes
    public int ApiCallsCount { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
