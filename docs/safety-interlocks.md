# Safety Interlocks

Tele-operation is disabled by default.

Recovery Mode must be explicitly entered before manual controls are available. The UI requires a confirmation dialog stating that manual recovery can affect a physical robot and that the area must be clear and authorized.

The backend enforces the same rule. `POST /api/robots/{id}/teleop-command` returns `409 Conflict` unless the robot status is `RecoveryMode`.

Emergency Stop is always available for a selected robot and immediately sets speed to zero.

Exiting Recovery Mode returns the robot to `Idle`, clears fault code and message fields, and resolves any active incident for that robot.
