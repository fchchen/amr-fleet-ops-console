export type RobotStatus =
  | 'Idle'
  | 'RunningMission'
  | 'Charging'
  | 'Blocked'
  | 'Faulted'
  | 'Offline'
  | 'EmergencyStopped'
  | 'RecoveryMode';

export type IncidentStatus = 'Open' | 'Acknowledged' | 'Resolved';
export type TeleopCommand = 'Stop' | 'Forward' | 'Backward' | 'Left' | 'Right';

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

export interface Mission {
  id: string;
  robotId: string;
  pickupPoint: string;
  dropoffPoint: string;
  priority: string;
  status: string;
  progressPercent: number;
  createdAtUtc: string;
  completedAtUtc?: string | null;
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
  status: IncidentStatus;
  createdAtUtc: string;
  acknowledgedAtUtc?: string | null;
  resolvedAtUtc?: string | null;
}
