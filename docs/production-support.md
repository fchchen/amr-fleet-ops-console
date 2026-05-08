# Production Support

The console exposes operational signals that matter during factory support:

- Heartbeat timestamp for every non-offline robot
- Status and speed visibility in fleet cards and diagnostics
- Fault code and fault message visibility for blocked or faulted robots
- Incident acknowledgement workflow for operator handoff
- Recovery controls separated from normal mission planning

The backend separates command handling, fleet state, and telemetry simulation so production implementations can add:

- Structured logs and metrics
- Persistent incident and mission history
- Alerting on missed heartbeats or repeated faults
- ROS 2/DDS or factory system telemetry adapters
- Role-based access control and operator audit logs
