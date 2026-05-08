using AmrFleet.Api.Models;

namespace AmrFleet.Api.Services;

public class RobotCommandService(IRobotFleetStore store)
{
    private static readonly RobotStatus[] RecoveryAllowedStatuses =
    [
        RobotStatus.Blocked,
        RobotStatus.Faulted,
        RobotStatus.EmergencyStopped,
        RobotStatus.Offline
    ];

    public bool EmergencyStop(string robotId)
    {
        var found = false;

        store.Update(state =>
        {
            var robot = state.Robots.SingleOrDefault(item => item.Id == robotId);
            if (robot is null)
            {
                return;
            }

            robot.Status = RobotStatus.EmergencyStopped;
            robot.Speed = 0;
            robot.LastHeartbeatUtc = DateTime.UtcNow;
            found = true;
        });

        return found;
    }

    public CommandResult EnterRecoveryMode(string robotId)
    {
        var result = CommandResult.NotFound();

        store.Update(state =>
        {
            var robot = state.Robots.SingleOrDefault(item => item.Id == robotId);
            if (robot is null)
            {
                return;
            }

            if (!RecoveryAllowedStatuses.Contains(robot.Status))
            {
                result = CommandResult.Conflict("Recovery mode is only allowed from Blocked, Faulted, EmergencyStopped, or Offline states.");
                return;
            }

            robot.Status = RobotStatus.RecoveryMode;
            robot.Speed = 0;
            robot.LastHeartbeatUtc = DateTime.UtcNow;
            result = CommandResult.Success();
        });

        return result;
    }

    public bool ExitRecoveryMode(string robotId)
    {
        var found = false;

        store.Update(state =>
        {
            var robot = state.Robots.SingleOrDefault(item => item.Id == robotId);
            if (robot is null)
            {
                return;
            }

            robot.Status = RobotStatus.Idle;
            robot.MissionId = null;
            robot.Speed = 0;
            robot.FaultCode = null;
            robot.FaultMessage = null;
            robot.LastHeartbeatUtc = DateTime.UtcNow;
            found = true;
        });

        if (found)
        {
            store.ResolveOpenIncidentForRobot(robotId);
        }

        return found;
    }

    public CommandResult Teleop(string robotId, TeleopCommand command)
    {
        var result = CommandResult.NotFound();

        store.Update(state =>
        {
            var robot = state.Robots.SingleOrDefault(item => item.Id == robotId);
            if (robot is null)
            {
                return;
            }

            if (robot.Status != RobotStatus.RecoveryMode)
            {
                result = CommandResult.Conflict("Tele-operation commands require RecoveryMode.");
                return;
            }

            switch (command)
            {
                case TeleopCommand.Stop:
                    robot.Speed = 0;
                    break;
                case TeleopCommand.Forward:
                    robot.Y = Math.Max(4, robot.Y - 2);
                    robot.Speed = 0.4;
                    break;
                case TeleopCommand.Backward:
                    robot.Y = Math.Min(96, robot.Y + 2);
                    robot.Speed = 0.3;
                    break;
                case TeleopCommand.Left:
                    robot.Heading = NormalizeHeading(robot.Heading - 15);
                    robot.Speed = 0;
                    break;
                case TeleopCommand.Right:
                    robot.Heading = NormalizeHeading(robot.Heading + 15);
                    robot.Speed = 0;
                    break;
            }

            robot.LastHeartbeatUtc = DateTime.UtcNow;
            result = CommandResult.Success();
        });

        return result;
    }

    private static double NormalizeHeading(double heading) => (heading + 360) % 360;
}

public record CommandResult(bool IsSuccess, bool IsNotFound, string? Error)
{
    public static CommandResult Success() => new(true, false, null);
    public static CommandResult NotFound() => new(false, true, "Robot not found.");
    public static CommandResult Conflict(string error) => new(false, false, error);
}
