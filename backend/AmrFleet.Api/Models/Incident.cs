namespace AmrFleet.Api.Models;

public class Incident
{
    public string Id { get; set; } = "";
    public string RobotId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
