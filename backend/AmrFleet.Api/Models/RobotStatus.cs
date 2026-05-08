namespace AmrFleet.Api.Models;

public enum RobotStatus
{
    Idle,
    RunningMission,
    Charging,
    Blocked,
    Faulted,
    Offline,
    EmergencyStopped,
    RecoveryMode
}
