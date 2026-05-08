using AmrFleet.Api.Models;

namespace AmrFleet.Api.Services;

public class FleetState
{
    public List<Robot> Robots { get; } = [];
    public List<Mission> Missions { get; } = [];
    public List<Incident> Incidents { get; } = [];
    public int NextMissionNumber { get; set; } = 1005;
}
