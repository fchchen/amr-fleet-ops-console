using AmrFleet.Api.Models;
using AmrFleet.Api.Options;
using Microsoft.Extensions.Options;

namespace AmrFleet.Api.Services;

public class RobotCommandService(
    IRobotFleetStore store,
    IOptions<FleetConsoleOptions> options,
    ILogger<RobotCommandService> logger)
{
    private const double ForwardSpeed = 0.4;
    private const double BackwardSpeed = 0.3;
    private const double TeleopStep = 2;
    private const double HeadingStepDegrees = 15;

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

        if (found)
        {
            logger.LogWarning("Emergency stop issued for {RobotId}.", robotId);
        }

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

        if (result.IsSuccess)
        {
            logger.LogWarning("Recovery mode entered for {RobotId}.", robotId);
        }

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
            logger.LogInformation("Recovery mode exited for {RobotId}.", robotId);
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
                    MoveRobotFrame(robot, TeleopStep);
                    robot.Speed = ForwardSpeed;
                    break;
                case TeleopCommand.Backward:
                    MoveRobotFrame(robot, -TeleopStep);
                    robot.Speed = BackwardSpeed;
                    break;
                case TeleopCommand.Left:
                    robot.Heading = NormalizeHeading(robot.Heading - HeadingStepDegrees);
                    robot.Speed = 0;
                    break;
                case TeleopCommand.Right:
                    robot.Heading = NormalizeHeading(robot.Heading + HeadingStepDegrees);
                    robot.Speed = 0;
                    break;
            }

            robot.LastHeartbeatUtc = DateTime.UtcNow;
            result = CommandResult.Success();
        });

        if (result.IsSuccess)
        {
            logger.LogInformation("Teleop command {Command} issued for {RobotId}.", command, robotId);
        }

        return result;
    }

    private void MoveRobotFrame(Robot robot, double step)
    {
        var consoleOptions = options.Value;
        robot.X = Clamp(robot.X + Math.Cos(ToRadians(robot.Heading)) * step, consoleOptions);
        robot.Y = Clamp(robot.Y + Math.Sin(ToRadians(robot.Heading)) * step, consoleOptions);
    }

    private static double Clamp(double value, FleetConsoleOptions options)
    {
        return Math.Min(options.MapMaximumCoordinate, Math.Max(options.MapMinimumCoordinate, value));
    }

    private static double NormalizeHeading(double heading) => (heading + 360) % 360;

    private static double ToRadians(double degrees) => Math.PI * degrees / 180.0;
}

public record CommandResult(bool IsSuccess, bool IsNotFound, string? Error)
{
    public static CommandResult Success() => new(true, false, null);
    public static CommandResult NotFound() => new(false, true, "Robot not found.");
    public static CommandResult Conflict(string error) => new(false, false, error);
}
