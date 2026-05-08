using AmrFleet.Api.Models;
using AmrFleet.Api.Options;
using AmrFleet.Api.Services;
using Microsoft.Extensions.Options;

namespace AmrFleet.Api.Simulators;

public class RobotMovementSimulator(IRobotFleetStore store, IOptions<FleetConsoleOptions> options)
{
    private const double RunningMissionMinimumSpeed = 1.0;
    private const double RunningMissionBatteryDrain = 0.03;
    private const double RunningMissionStep = 0.8;
    private const int MaxDemoProgressPercent = 95;

    public void Tick()
    {
        store.Update(state =>
        {
            var now = DateTime.UtcNow;

            foreach (var robot in state.Robots)
            {
                if (robot.Status != RobotStatus.Offline)
                {
                    robot.LastHeartbeatUtc = now;
                }

                if (robot.Status != RobotStatus.RunningMission)
                {
                    continue;
                }

                robot.Speed = Math.Max(robot.Speed, RunningMissionMinimumSpeed);
                robot.BatteryPercent = Math.Max(0, robot.BatteryPercent - RunningMissionBatteryDrain);
                robot.X = Wrap(robot.X + Math.Cos(ToRadians(robot.Heading)) * RunningMissionStep);
                robot.Y = Wrap(robot.Y + Math.Sin(ToRadians(robot.Heading)) * RunningMissionStep);

                var mission = state.Missions.SingleOrDefault(item => item.Id == robot.MissionId);
                if (mission is null || mission.Status != MissionStatus.InProgress)
                {
                    continue;
                }

                mission.ProgressPercent = Math.Min(MaxDemoProgressPercent, mission.ProgressPercent + 1);
            }
        });
    }

    private double Wrap(double value)
    {
        var consoleOptions = options.Value;

        if (value > consoleOptions.MapMaximumCoordinate - 2)
        {
            return consoleOptions.MapMinimumCoordinate + 2;
        }

        if (value < consoleOptions.MapMinimumCoordinate + 2)
        {
            return consoleOptions.MapMaximumCoordinate - 2;
        }

        return value;
    }

    private static double ToRadians(double degrees) => Math.PI * degrees / 180.0;
}
