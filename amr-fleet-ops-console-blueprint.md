# Blueprint: `amr-fleet-ops-console`

## 1. Project goal

Build a simulated **Autonomous Mobile Robot fleet operations console** for manufacturing floor operators.

The app should demonstrate:

```text
Angular operator-facing UI
ASP.NET Core backend
SignalR real-time telemetry
SVG factory floor map with moving robots
Mission assignment workflow
Blocked robot incident scenario
Recovery mode with safety interlock
Docker Compose local deployment
Strong README and architecture docs
```

This project is designed to match a Senior Application Software Engineer role focused on AMR operator consoles, fleet tools, map visualizations, mission planning UIs, diagnostics dashboards, recovery workflows, safety interlocks, observability, Docker, and production support.

---

## 2. Recommended technology stack

### Frontend

```text
Angular
TypeScript
RxJS
Angular Material
SVG-based 2D factory map
```

### Backend

```text
ASP.NET Core Web API
C#
SignalR
BackgroundService telemetry simulator
In-memory state store
```

### DevOps

```text
Docker
Docker Compose
GitHub Actions
README
Architecture docs
```

Do **not** use a database in the first version. Keep robot, mission, and incident state in memory.

---

## 3. Final repo structure

```text
amr-fleet-ops-console/
│
├── frontend/
│   └── amr-fleet-ui/
│       ├── src/
│       │   ├── app/
│       │   │   ├── core/
│       │   │   │   ├── models/
│       │   │   │   ├── services/
│       │   │   │   └── signalr/
│       │   │   ├── features/
│       │   │   │   ├── dashboard/
│       │   │   │   ├── factory-map/
│       │   │   │   ├── mission-planner/
│       │   │   │   ├── diagnostics/
│       │   │   │   ├── incidents/
│       │   │   │   └── recovery/
│       │   │   ├── shared/
│       │   │   ├── app.component.ts
│       │   │   ├── app.component.html
│       │   │   └── app.routes.ts
│       │   └── environments/
│       ├── Dockerfile
│       └── package.json
│
├── backend/
│   └── AmrFleet.Api/
│       ├── Controllers/
│       │   ├── RobotsController.cs
│       │   ├── MissionsController.cs
│       │   └── IncidentsController.cs
│       ├── Hubs/
│       │   └── RobotTelemetryHub.cs
│       ├── Models/
│       │   ├── Robot.cs
│       │   ├── Mission.cs
│       │   ├── Incident.cs
│       │   ├── RobotTelemetryDto.cs
│       │   ├── TeleopCommandRequest.cs
│       │   └── MissionCreateRequest.cs
│       ├── Services/
│       │   ├── IRobotFleetStore.cs
│       │   ├── InMemoryRobotFleetStore.cs
│       │   ├── RobotCommandService.cs
│       │   └── MissionService.cs
│       ├── Simulators/
│       │   ├── RobotTelemetryBackgroundService.cs
│       │   └── RobotMovementSimulator.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
│
├── docs/
│   ├── architecture.md
│   ├── safety-interlocks.md
│   └── production-support.md
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── docker-compose.yml
├── README.md
└── .gitignore
```

Do **not** create a public file called `interview-talking-points.md`.

---

## 4. Main user story

The app should support this demo flow:

```text
1. User opens dashboard.
2. User sees 4 simulated AMRs.
3. Robots update live through SignalR.
4. User sees robots moving on an SVG factory map.
5. User assigns a mission to AMR-001.
6. AMR-001 changes to RunningMission and moves toward destination.
7. AMR-003 is already Blocked because of an obstacle.
8. User opens AMR-003 incident.
9. User acknowledges the incident.
10. User enters Recovery Mode.
11. Safety confirmation dialog appears.
12. Manual tele-op buttons become enabled only after Recovery Mode is active.
13. User sends Stop / Forward / Back command.
14. User clears recovery mode.
15. AMR-003 returns to Idle.
```

This is the most important scenario.

---

## 5. Backend design

### 5.1 Enums

Create these enums.

```csharp
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

public enum MissionStatus
{
    Pending,
    Assigned,
    InProgress,
    Completed,
    Cancelled,
    Failed
}

public enum IncidentStatus
{
    Open,
    Acknowledged,
    Resolved
}

public enum TeleopCommand
{
    Stop,
    Forward,
    Backward,
    Left,
    Right
}
```

---

### 5.2 Backend models

#### `Robot.cs`

```csharp
public class Robot
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public RobotStatus Status { get; set; }
    public string? MissionId { get; set; }
    public double BatteryPercent { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public double Heading { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public string? FaultCode { get; set; }
    public string? FaultMessage { get; set; }
}
```

#### `Mission.cs`

```csharp
public class Mission
{
    public string Id { get; set; } = "";
    public string RobotId { get; set; } = "";
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public MissionStatus Status { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
```

#### `Incident.cs`

```csharp
public class Incident
{
    public string Id { get; set; } = "";
    public string RobotId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}
```

#### `RobotTelemetryDto.cs`

```csharp
public class RobotTelemetryDto
{
    public string RobotId { get; set; } = "";
    public string RobotName { get; set; } = "";
    public RobotStatus Status { get; set; }
    public string? MissionId { get; set; }
    public double BatteryPercent { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public double Heading { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public string? FaultCode { get; set; }
    public string? FaultMessage { get; set; }
}
```

#### `MissionCreateRequest.cs`

```csharp
public class MissionCreateRequest
{
    public string RobotId { get; set; } = "";
    public string PickupPoint { get; set; } = "";
    public string DropoffPoint { get; set; } = "";
    public string Priority { get; set; } = "Normal";
}
```

#### `TeleopCommandRequest.cs`

```csharp
public class TeleopCommandRequest
{
    public TeleopCommand Command { get; set; }
}
```

---

## 6. Initial robot state

Seed exactly 4 robots:

```text
AMR-001
Status: RunningMission
Battery: 82
MissionId: MIS-1001
X: 20
Y: 25
Speed: 1.2

AMR-002
Status: Charging
Battery: 31
MissionId: null
X: 8
Y: 80
Speed: 0

AMR-003
Status: Blocked
Battery: 67
MissionId: MIS-1003
X: 70
Y: 45
Speed: 0
FaultCode: OBSTACLE_DETECTED
FaultMessage: Obstacle detected near Warehouse Zone 2

AMR-004
Status: Idle
Battery: 91
MissionId: null
X: 50
Y: 70
Speed: 0
```

Seed one incident for AMR-003:

```text
INC-3001
RobotId: AMR-003
Title: Obstacle detected
Description: AMR-003 detected an obstacle near Warehouse Zone 2 and stopped safely.
Status: Open
```

---

## 7. Backend APIs

### Robots

```text
GET /api/robots
GET /api/robots/{id}
POST /api/robots/{id}/emergency-stop
POST /api/robots/{id}/enter-recovery-mode
POST /api/robots/{id}/exit-recovery-mode
POST /api/robots/{id}/teleop-command
```

Rules:

```text
emergency-stop:
  Set status = EmergencyStopped
  Set speed = 0

enter-recovery-mode:
  Allowed only if status is Blocked, Faulted, EmergencyStopped, or Offline
  Set status = RecoveryMode
  Set speed = 0

exit-recovery-mode:
  Set status = Idle
  Clear fault code and fault message
  Resolve open incident for this robot if exists

teleop-command:
  Only allowed if status == RecoveryMode
  Otherwise return HTTP 409 Conflict
  Stop sets speed = 0
  Forward changes Y or X slightly
  Backward changes Y or X slightly
  Left changes heading -15
  Right changes heading +15
```

This rule is critical:

```text
Tele-operation commands must fail unless robot status == RecoveryMode.
```

---

### Missions

```text
GET /api/missions
POST /api/missions
POST /api/missions/{id}/cancel
```

Rules:

```text
POST /api/missions:
  Robot must be Idle
  Create mission ID like MIS-1005
  Set mission status = InProgress
  Set robot status = RunningMission
  Set robot.MissionId = new mission ID
```

---

### Incidents

```text
GET /api/incidents
GET /api/incidents?status=Open
POST /api/incidents/{id}/acknowledge
POST /api/incidents/{id}/resolve
```

Rules:

```text
acknowledge:
  Set status = Acknowledged
  Set AcknowledgedAtUtc = now

resolve:
  Set status = Resolved
  Set ResolvedAtUtc = now
```

---

## 8. SignalR design

### Hub

```csharp
public class RobotTelemetryHub : Hub
{
}
```

### Event name

Broadcast event:

```text
robotTelemetryUpdated
```

Payload:

```json
[
  {
    "robotId": "AMR-001",
    "robotName": "AMR-001",
    "status": "RunningMission",
    "missionId": "MIS-1001",
    "batteryPercent": 82,
    "x": 21,
    "y": 26,
    "speed": 1.2,
    "heading": 90,
    "lastHeartbeatUtc": "2026-05-08T12:00:00Z",
    "faultCode": null,
    "faultMessage": null
  }
]
```

### Background service behavior

Every 1 second:

```text
1. Update LastHeartbeatUtc for all non-offline robots.
2. Move robots with status RunningMission slightly across map.
3. Decrease battery slowly for RunningMission robots.
4. Keep AMR-003 blocked until user enters and exits Recovery Mode.
5. Broadcast all robot telemetry via SignalR.
```

---

## 9. Frontend design

### 9.1 Layout

Single-page dashboard is enough.

Use a 2-column layout:

```text
+----------------------------------------------------------+
| Header: AMR Fleet Ops Console                            |
+------------------------------+---------------------------+
| Left: Fleet + Mission Panel   | Right: Factory Map        |
|                              |                           |
| Robot Cards                  | SVG Map                   |
| Mission Form                 | Moving Robot Icons        |
| Incidents                    |                           |
+------------------------------+---------------------------+
| Bottom: Diagnostics + Recovery Controls                  |
+----------------------------------------------------------+
```

---

### 9.2 Angular models

Create TypeScript models matching backend:

```ts
export type RobotStatus =
  | 'Idle'
  | 'RunningMission'
  | 'Charging'
  | 'Blocked'
  | 'Faulted'
  | 'Offline'
  | 'EmergencyStopped'
  | 'RecoveryMode';

export interface RobotTelemetry {
  robotId: string;
  robotName: string;
  status: RobotStatus;
  missionId?: string | null;
  batteryPercent: number;
  x: number;
  y: number;
  speed: number;
  heading: number;
  lastHeartbeatUtc: string;
  faultCode?: string | null;
  faultMessage?: string | null;
}

export interface MissionCreateRequest {
  robotId: string;
  pickupPoint: string;
  dropoffPoint: string;
  priority: string;
}

export interface Incident {
  id: string;
  robotId: string;
  title: string;
  description: string;
  status: 'Open' | 'Acknowledged' | 'Resolved';
  createdAtUtc: string;
  acknowledgedAtUtc?: string | null;
  resolvedAtUtc?: string | null;
}
```

---

### 9.3 Angular services

Create:

```text
RobotApiService
MissionApiService
IncidentApiService
RobotTelemetrySignalRService
```

#### `RobotTelemetrySignalRService`

Responsibilities:

```text
Connect to /hubs/robotTelemetry
Listen for robotTelemetryUpdated
Expose robots$ as BehaviorSubject<RobotTelemetry[]>
Auto-reconnect
```

---

## 10. Angular components

### 10.1 `DashboardComponent`

Responsibilities:

```text
Main page container
Subscribes to robots$
Tracks selected robot
Passes selected robot to diagnostics and recovery components
```

---

### 10.2 `FleetSummaryComponent`

Display robot cards.

Each card shows:

```text
Robot name
Status badge
Battery %
Mission ID
Fault message if any
Last heartbeat
```

Clicking a card selects robot.

Status colors:

```text
Idle: neutral
RunningMission: blue
Charging: purple
Blocked/Faulted/EmergencyStopped: red
RecoveryMode: orange
Offline: gray
```

---

### 10.3 `FactoryMapComponent`

Use SVG, not canvas.

SVG coordinate system:

```html
<svg viewBox="0 0 100 100">
```

Show:

```text
Factory boundary
Charging station
Pickup A
Dropoff B
Warehouse Zone 2
Restricted zone
Robot icons
Route line for RunningMission robot
Obstacle marker near AMR-003
```

Robot marker:

```text
Circle
Robot name text
Small heading arrow
Color based on status
```

Clicking robot marker selects robot.

Acceptance criteria:

```text
Robots move visually when SignalR telemetry changes.
AMR-003 remains blocked near obstacle.
Selected robot is visually highlighted.
```

---

### 10.4 `MissionPlannerComponent`

Form fields:

```text
Robot dropdown
Pickup dropdown
Dropoff dropdown
Priority dropdown
Assign Mission button
```

Pickup/dropoff options:

```text
Line A
Line B
Warehouse Zone 1
Warehouse Zone 2
Charging Station
Inspection Bay
```

Rules:

```text
Only Idle robots can receive new missions.
Disable Assign Mission button if selected robot is not Idle.
After mission assignment, refresh robots/missions or rely on SignalR.
```

---

### 10.5 `IncidentsComponent`

Show open/acknowledged incidents.

Each incident card:

```text
Title
Robot ID
Description
Status
Acknowledge button
```

Rules:

```text
Acknowledge button visible only when status == Open.
```

---

### 10.6 `DiagnosticsComponent`

For selected robot, show:

```text
Battery
Speed
Heading
Status
Mission ID
Last heartbeat
Fault code
Fault message
```

Use simple cards or progress bars.

---

### 10.7 `RecoveryControlsComponent`

This is the most important component.

Show selected robot.

Buttons:

```text
Emergency Stop
Enter Recovery Mode
Exit Recovery Mode
Forward
Backward
Left
Right
Stop
```

Rules:

```text
Emergency Stop always enabled for selected robot.
Enter Recovery Mode enabled only when status is Blocked, Faulted, EmergencyStopped, or Offline.
Tele-op buttons enabled only when status == RecoveryMode.
Exit Recovery Mode enabled only when status == RecoveryMode.
```

Safety confirmation:

When user clicks `Enter Recovery Mode`, show confirmation dialog:

```text
Manual recovery mode can affect a physical robot.
Confirm that the area is clear and recovery is authorized.
```

Only call backend after confirmation.

When tele-op command fails because robot is not in RecoveryMode, show error message.

Acceptance criteria:

```text
Manual controls are disabled until RecoveryMode is active.
Backend also rejects tele-op commands unless RecoveryMode is active.
This safety rule must exist both in UI and backend.
```

---

## 11. Styling requirements

Keep it professional and manufacturing-console-like.

Use:

```text
Dark header
Clean cards
Status badges
Readable font sizes
No fancy animations except smooth robot movement if easy
```

Avoid overdesign.

The app should look like an internal industrial dashboard, not a consumer website.

---

## 12. Docker requirements

### `docker-compose.yml`

Should start:

```text
backend on http://localhost:5000
frontend on http://localhost:4200 or http://localhost:8080
```

Suggested compose:

```yaml
services:
  api:
    build:
      context: ./backend/AmrFleet.Api
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080

  ui:
    build:
      context: ./frontend/amr-fleet-ui
    ports:
      - "4200:80"
    depends_on:
      - api
```

Frontend Dockerfile can build Angular and serve with nginx.

Backend Dockerfile should publish ASP.NET Core app.

---

## 13. README requirements

The README must include:

```text
Project purpose
Why this project exists
Feature list
Architecture diagram
Screenshots placeholder
How to run locally
How to run with Docker
Demo scenario
Production considerations
Future enhancements
```

Use this exact framing:

```markdown
## Why this project exists

This project demonstrates the operator-facing application layer for an AMR fleet management system in a manufacturing environment.

It focuses on real-time fleet visibility, map-based mission planning, diagnostics, incident handling, and recovery workflows with safety interlocks.

The robotics middleware is simulated, but the backend is structured so the simulator could be replaced by an adapter to ROS 2/DDS or a factory system.
```

---

## 14. Docs requirements

### `docs/architecture.md`

Include:

```text
Angular UI
REST APIs
SignalR telemetry stream
ASP.NET Core services
In-memory fleet store
Telemetry simulator
Future ROS 2/DDS adapter
```

Include diagram:

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

### `docs/safety-interlocks.md`

Explain:

```text
Tele-op controls are disabled by default.
Recovery Mode must be explicitly entered.
A confirmation dialog is required.
Backend enforces the same rule.
Emergency Stop always sets speed to zero.
```

### `docs/production-support.md`

Explain:

```text
Heartbeat monitoring
Fault code visibility
Incident acknowledgment
Operator-friendly diagnostics
Separation of telemetry source from UI
Future logging/metrics possibilities
```

---

## 15. GitHub Actions

Create a simple CI workflow.

```yaml
name: CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet build backend/AmrFleet.Api/AmrFleet.Api.csproj

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
      - run: npm ci
        working-directory: frontend/amr-fleet-ui
      - run: npm run build
        working-directory: frontend/amr-fleet-ui
```

If Codex chooses .NET 8 instead of .NET 9, that is also fine. Use a stable LTS version if preferred.

---

## 16. Acceptance criteria

The implementation is successful when:

```text
1. docker compose up starts both frontend and backend.
2. User can open the Angular UI.
3. Four AMRs appear on dashboard.
4. Robot telemetry updates live without page refresh.
5. Robots move on SVG factory map.
6. User can assign a mission to an Idle robot.
7. AMR-003 starts in Blocked status with an obstacle incident.
8. User can acknowledge the AMR-003 incident.
9. User cannot use tele-op controls until Recovery Mode is active.
10. Entering Recovery Mode requires confirmation.
11. Backend rejects tele-op command if robot is not in RecoveryMode.
12. User can exit Recovery Mode and AMR-003 becomes Idle.
13. README explains purpose, architecture, and demo flow.
14. CI builds backend and frontend.
```

---

## 17. Codex prompt to paste

You can paste this into Codex:

```text
Create a new full-stack demo repo named amr-fleet-ops-console.

Goal:
Build a simulated Autonomous Mobile Robot fleet operations console for manufacturing floor operators. The project should demonstrate Angular UI, ASP.NET Core backend, SignalR live telemetry, SVG factory map visualization, mission assignment, blocked robot incident handling, recovery mode with safety interlocks, Docker Compose, and strong documentation.

Use this stack:
- Frontend: Angular, TypeScript, RxJS, Angular Material, SVG map
- Backend: ASP.NET Core Web API, C#, SignalR, BackgroundService telemetry simulator
- State: in-memory only
- DevOps: Docker, Docker Compose, GitHub Actions

Create this repo structure:
[insert repo structure from blueprint]

Backend requirements:
- Create Robot, Mission, Incident, RobotTelemetryDto, MissionCreateRequest, TeleopCommandRequest models.
- Add RobotStatus, MissionStatus, IncidentStatus, TeleopCommand enums.
- Seed 4 robots: AMR-001 RunningMission, AMR-002 Charging, AMR-003 Blocked with OBSTACLE_DETECTED, AMR-004 Idle.
- Seed incident INC-3001 for AMR-003.
- Add RobotsController, MissionsController, IncidentsController.
- Add SignalR hub at /hubs/robotTelemetry.
- Add BackgroundService that broadcasts robotTelemetryUpdated every 1 second.
- RunningMission robots should move slightly on each tick.
- AMR-003 should stay blocked until recovery workflow resolves it.
- Teleop commands must be rejected unless robot status is RecoveryMode.
- Emergency stop always sets speed to zero.

Frontend requirements:
- Create Angular app under frontend/amr-fleet-ui.
- Use Angular Material or clean CSS components.
- Create Dashboard, FleetSummary, FactoryMap, MissionPlanner, Incidents, Diagnostics, and RecoveryControls components.
- Connect to SignalR and update dashboard/map live.
- FactoryMap must use SVG with viewBox 0 0 100 100.
- Show charging station, pickup/dropoff points, restricted zone, obstacle marker, and robot markers.
- Robot markers should move as telemetry changes.
- Mission planner should allow assigning a mission only to Idle robots.
- AMR-003 should show as Blocked with an open incident.
- Recovery controls should require confirmation before entering Recovery Mode.
- Teleop buttons must be disabled unless selected robot status is RecoveryMode.

Docker:
- Add backend Dockerfile.
- Add frontend Dockerfile that builds Angular and serves with nginx.
- Add docker-compose.yml so frontend and backend run together.

Docs:
- README.md with project purpose, features, architecture, how to run, demo scenario, and production considerations.
- docs/architecture.md
- docs/safety-interlocks.md
- docs/production-support.md
- Do not create public interview talking-points docs.

CI:
- Add GitHub Actions workflow to build backend and frontend.

Acceptance criteria:
- docker compose up starts the app.
- Four AMRs appear.
- Telemetry updates live.
- Robots move on SVG map.
- User can assign mission to idle robot.
- AMR-003 starts blocked.
- User can acknowledge incident.
- Recovery Mode requires confirmation.
- Teleop controls are disabled until RecoveryMode.
- Backend also rejects unsafe teleop commands.
- Exiting Recovery Mode clears AMR-003 fault and returns it to Idle.
- README is polished and professional.
```

---

## 18. Suggested implementation order for Codex

Give Codex smaller tasks in this order, rather than asking for everything at once:

```text
Task 1: Create backend models, seeded in-memory store, controllers, and SignalR hub.
Task 2: Add telemetry BackgroundService and robot movement simulator.
Task 3: Create Angular app with layout, services, and SignalR subscription.
Task 4: Add fleet dashboard and SVG factory map.
Task 5: Add mission planner.
Task 6: Add incidents and diagnostics panels.
Task 7: Add recovery controls with safety confirmation and backend enforcement.
Task 8: Add Dockerfiles and docker-compose.yml.
Task 9: Add README and docs.
Task 10: Add GitHub Actions CI.
```

Recommendation: do not ask Codex to build the whole repo in one huge pass. The best result will come from small pull-request-sized tasks.
