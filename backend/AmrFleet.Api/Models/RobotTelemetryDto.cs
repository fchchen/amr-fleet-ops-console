namespace AmrFleet.Api.Models;

public class RobotTelemetryDto
{
    public string RobotId { get; set; } = "";
    public string RobotName { get; set; } = "";
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

    public static RobotTelemetryDto FromRobot(Robot robot) => new()
    {
        RobotId = robot.Id,
        RobotName = robot.Name,
        Status = robot.Status,
        MissionId = robot.MissionId,
        BatteryPercent = Math.Round(robot.BatteryPercent, 1),
        X = Math.Round(robot.X, 1),
        Y = Math.Round(robot.Y, 1),
        Speed = Math.Round(robot.Speed, 1),
        Heading = Math.Round(robot.Heading, 0),
        LastHeartbeatUtc = robot.LastHeartbeatUtc,
        FaultCode = robot.FaultCode,
        FaultMessage = robot.FaultMessage
    };
}
