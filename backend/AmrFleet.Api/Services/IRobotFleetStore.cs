using AmrFleet.Api.Models;

namespace AmrFleet.Api.Services;

public interface IRobotFleetStore
{
    IReadOnlyList<Robot> GetRobots();
    Robot? GetRobot(string id);
    IReadOnlyList<Mission> GetMissions();
    Mission? GetMission(string id);
    IReadOnlyList<Incident> GetIncidents(IncidentStatus? status = null);
    Incident? GetIncident(string id);
    Mission AddMission(MissionCreateRequest request);
    bool CancelMission(string id);
    bool AcknowledgeIncident(string id);
    bool ResolveIncident(string id);
    bool ResolveOpenIncidentForRobot(string robotId);
    void Update(Action<FleetState> update);
}
