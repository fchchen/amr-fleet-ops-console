using AmrFleet.Api.Models;
using AmrFleet.Api.Services;

namespace AmrFleet.Api.Simulators;

public class RobotMovementSimulator(IRobotFleetStore store)
{
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

                robot.Speed = Math.Max(robot.Speed, 1.0);
                robot.BatteryPercent = Math.Max(0, robot.BatteryPercent - 0.03);
                robot.X = Wrap(robot.X + Math.Cos(ToRadians(robot.Heading)) * 0.8);
                robot.Y = Wrap(robot.Y + Math.Sin(ToRadians(robot.Heading)) * 0.8);

                var mission = state.Missions.SingleOrDefault(item => item.Id == robot.MissionId);
                if (mission is null || mission.Status != MissionStatus.InProgress)
                {
                    continue;
                }

                mission.ProgressPercent = Math.Min(95, mission.ProgressPercent + 1);
            }
        });
    }

    private static double Wrap(double value)
    {
        if (value > 94)
        {
            return 6;
        }

        if (value < 6)
        {
            return 94;
        }

        return value;
    }

    private static double ToRadians(double degrees) => Math.PI * degrees / 180.0;
}
