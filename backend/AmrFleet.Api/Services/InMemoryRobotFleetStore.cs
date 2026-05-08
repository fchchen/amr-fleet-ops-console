using AmrFleet.Api.Models;

namespace AmrFleet.Api.Services;

public class InMemoryRobotFleetStore : IRobotFleetStore
{
    private readonly object syncRoot = new();
    private readonly FleetState state = new();

    public InMemoryRobotFleetStore()
    {
        var now = DateTime.UtcNow;

        state.Robots.AddRange(
        [
            new Robot
            {
                Id = "AMR-001",
                Name = "AMR-001",
                Status = RobotStatus.RunningMission,
                BatteryPercent = 82,
                MissionId = "MIS-1001",
                X = 20,
                Y = 25,
                Speed = 1.2,
                Heading = 90,
                LastHeartbeatUtc = now
            },
            new Robot
            {
                Id = "AMR-002",
                Name = "AMR-002",
                Status = RobotStatus.Charging,
                BatteryPercent = 31,
                X = 8,
                Y = 80,
                Speed = 0,
                Heading = 0,
                LastHeartbeatUtc = now
            },
            new Robot
            {
                Id = "AMR-003",
                Name = "AMR-003",
                Status = RobotStatus.Blocked,
                BatteryPercent = 67,
                MissionId = "MIS-1003",
                X = 70,
                Y = 45,
                Speed = 0,
                Heading = 180,
                LastHeartbeatUtc = now,
                FaultCode = "OBSTACLE_DETECTED",
                FaultMessage = "Obstacle detected near Warehouse Zone 2"
            },
            new Robot
            {
                Id = "AMR-004",
                Name = "AMR-004",
                Status = RobotStatus.Idle,
                BatteryPercent = 91,
                X = 50,
                Y = 70,
                Speed = 0,
                Heading = 270,
                LastHeartbeatUtc = now
            }
        ]);

        state.Missions.AddRange(
        [
            new Mission
            {
                Id = "MIS-1001",
                RobotId = "AMR-001",
                PickupPoint = "Line A",
                DropoffPoint = "Inspection Bay",
                Priority = "Normal",
                Status = MissionStatus.InProgress,
                ProgressPercent = 35,
                CreatedAtUtc = now.AddMinutes(-18)
            },
            new Mission
            {
                Id = "MIS-1003",
                RobotId = "AMR-003",
                PickupPoint = "Warehouse Zone 2",
                DropoffPoint = "Line B",
                Priority = "High",
                Status = MissionStatus.InProgress,
                ProgressPercent = 52,
                CreatedAtUtc = now.AddMinutes(-9)
            }
        ]);

        state.Incidents.Add(new Incident
        {
            Id = "INC-3001",
            RobotId = "AMR-003",
            Title = "Obstacle detected",
            Description = "AMR-003 detected an obstacle near Warehouse Zone 2 and stopped safely.",
            Status = IncidentStatus.Open,
            CreatedAtUtc = now.AddMinutes(-6)
        });
    }

    public IReadOnlyList<Robot> GetRobots()
    {
        lock (syncRoot)
        {
            return state.Robots.Select(CloneRobot).ToList();
        }
    }

    public Robot? GetRobot(string id)
    {
        lock (syncRoot)
        {
            return state.Robots.Where(robot => robot.Id == id).Select(CloneRobot).SingleOrDefault();
        }
    }

    public IReadOnlyList<Mission> GetMissions()
    {
        lock (syncRoot)
        {
            return state.Missions.Select(CloneMission).ToList();
        }
    }

    public Mission? GetMission(string id)
    {
        lock (syncRoot)
        {
            return state.Missions.Where(mission => mission.Id == id).Select(CloneMission).SingleOrDefault();
        }
    }

    public IReadOnlyList<Incident> GetIncidents(IncidentStatus? status = null)
    {
        lock (syncRoot)
        {
            return state.Incidents
                .Where(incident => status is null || incident.Status == status)
                .Select(CloneIncident)
                .ToList();
        }
    }

    public Incident? GetIncident(string id)
    {
        lock (syncRoot)
        {
            return state.Incidents.Where(incident => incident.Id == id).Select(CloneIncident).SingleOrDefault();
        }
    }

    public Mission AddMission(MissionCreateRequest request)
    {
        lock (syncRoot)
        {
            var robot = state.Robots.SingleOrDefault(item => item.Id == request.RobotId)
                ?? throw new MissionCreateException(MissionCreateFailure.RobotNotFound, "Robot not found.");

            if (robot.Status != RobotStatus.Idle)
            {
                throw new MissionCreateException(MissionCreateFailure.RobotUnavailable, "Robot must be idle before assigning a mission.");
            }

            var mission = new Mission
            {
                Id = $"MIS-{state.NextMissionNumber++}",
                RobotId = robot.Id,
                PickupPoint = request.PickupPoint,
                DropoffPoint = request.DropoffPoint,
                Priority = request.Priority,
                Status = MissionStatus.InProgress,
                ProgressPercent = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            state.Missions.Add(mission);
            robot.Status = RobotStatus.RunningMission;
            robot.MissionId = mission.Id;
            robot.Speed = 1.1;

            return CloneMission(mission);
        }
    }

    public bool CancelMission(string id)
    {
        lock (syncRoot)
        {
            var mission = state.Missions.SingleOrDefault(item => item.Id == id);
            if (mission is null)
            {
                return false;
            }

            mission.Status = MissionStatus.Cancelled;
            mission.CompletedAtUtc = DateTime.UtcNow;

            var robot = state.Robots.SingleOrDefault(item => item.Id == mission.RobotId);
            if (robot is not null && robot.MissionId == mission.Id)
            {
                robot.MissionId = null;
                robot.Status = RobotStatus.Idle;
                robot.Speed = 0;
            }

            return true;
        }
    }

    public bool AcknowledgeIncident(string id)
    {
        lock (syncRoot)
        {
            var incident = state.Incidents.SingleOrDefault(item => item.Id == id);
            if (incident is null)
            {
                return false;
            }

            if (incident.Status == IncidentStatus.Open)
            {
                incident.Status = IncidentStatus.Acknowledged;
                incident.AcknowledgedAtUtc = DateTime.UtcNow;
            }

            return true;
        }
    }

    public bool ResolveIncident(string id)
    {
        lock (syncRoot)
        {
            var incident = state.Incidents.SingleOrDefault(item => item.Id == id);
            if (incident is null)
            {
                return false;
            }

            ResolveIncident(incident);
            return true;
        }
    }

    public bool ResolveOpenIncidentForRobot(string robotId)
    {
        lock (syncRoot)
        {
            var incident = state.Incidents.FirstOrDefault(item =>
                item.RobotId == robotId && item.Status != IncidentStatus.Resolved);

            if (incident is null)
            {
                return false;
            }

            ResolveIncident(incident);
            return true;
        }
    }

    public void Update(Action<FleetState> update)
    {
        lock (syncRoot)
        {
            update(state);
        }
    }

    private static void ResolveIncident(Incident incident)
    {
        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedAtUtc = DateTime.UtcNow;
    }

    private static Robot CloneRobot(Robot robot) => new()
    {
        Id = robot.Id,
        Name = robot.Name,
        Status = robot.Status,
        MissionId = robot.MissionId,
        BatteryPercent = robot.BatteryPercent,
        X = robot.X,
        Y = robot.Y,
        Speed = robot.Speed,
        Heading = robot.Heading,
        LastHeartbeatUtc = robot.LastHeartbeatUtc,
        FaultCode = robot.FaultCode,
        FaultMessage = robot.FaultMessage
    };

    private static Mission CloneMission(Mission mission) => new()
    {
        Id = mission.Id,
        RobotId = mission.RobotId,
        PickupPoint = mission.PickupPoint,
        DropoffPoint = mission.DropoffPoint,
        Priority = mission.Priority,
        Status = mission.Status,
        ProgressPercent = mission.ProgressPercent,
        CreatedAtUtc = mission.CreatedAtUtc,
        CompletedAtUtc = mission.CompletedAtUtc
    };

    private static Incident CloneIncident(Incident incident) => new()
    {
        Id = incident.Id,
        RobotId = incident.RobotId,
        Title = incident.Title,
        Description = incident.Description,
        Status = incident.Status,
        CreatedAtUtc = incident.CreatedAtUtc,
        AcknowledgedAtUtc = incident.AcknowledgedAtUtc,
        ResolvedAtUtc = incident.ResolvedAtUtc
    };
}
