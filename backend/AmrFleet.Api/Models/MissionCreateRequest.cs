namespace AmrFleet.Api.Models;

public class MissionCreateRequest
{
    public string RobotId { get; set; } = "";
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public string Priority { get; set; } = "Normal";
}
