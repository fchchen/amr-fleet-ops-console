namespace AmrFleet.Api.Models;

public class Robot
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public RobotStatus Status { get; set; }
    public string? MissionId { get; set; }
    public double BatteryPercent { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public double Heading { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public string? FaultCode { get; set; }
    public string? FaultMessage { get; set; }
}
