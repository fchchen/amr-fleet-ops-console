namespace AmrFleet.Api.Models;

public class Mission
{
    public string Id { get; set; } = "";
    public string RobotId { get; set; } = "";
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public MissionStatus Status { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
