# Architecture

The app is split into an Angular operator UI and an ASP.NET Core API.

```text
+-----------------------+
| Angular Operator UI   |
+-----------+-----------+
            |
            | REST + SignalR
            |
+-----------v-----------+
| ASP.NET Core API      |
| Robot/Mission/Incident|
+-----------+-----------+
            |
            | IRobotTelemetrySource
            |
+-----------v-----------+
| Simulator today       |
| ROS 2/DDS adapter later|
+-----------------------+
```

## Components

- Angular UI renders the dashboard, map, mission workflow, diagnostics, incidents, and recovery controls.
- REST APIs handle command-style operations such as mission assignment and incident acknowledgement.
- SignalR streams `robotTelemetryUpdated` events to keep the map and fleet cards live.
- ASP.NET Core services hold robot, mission, and incident behavior behind an in-memory fleet store.
- The telemetry simulator updates robot positions and heartbeats every second.

The simulator is intentionally isolated from the UI contract so a future ROS 2/DDS adapter can publish the same telemetry shape.
