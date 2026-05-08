using AmrFleet.Api.Models;

namespace AmrFleet.Api.Services;

public class MissionService(IRobotFleetStore store)
{
    public Mission CreateMission(MissionCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RobotId) ||
            string.IsNullOrWhiteSpace(request.PickupPoint) ||
            string.IsNullOrWhiteSpace(request.DropoffPoint))
        {
            throw new InvalidOperationException("Robot, pickup point, and dropoff point are required.");
        }

        return store.AddMission(request);
    }

    public bool CancelMission(string id) => store.CancelMission(id);
}
