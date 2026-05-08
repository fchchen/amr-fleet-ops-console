import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import { RobotTelemetry, TeleopCommand } from '../models/fleet.models';

@Injectable({ providedIn: 'root' })
export class RobotApiService {
  constructor(private readonly http: HttpClient) {}

  getRobots(): Observable<RobotTelemetry[]> {
    return this.http.get<RobotTelemetry[]>(`${API_BASE_URL}/api/robots`);
  }

  emergencyStop(robotId: string): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/api/robots/${robotId}/emergency-stop`, {});
  }

  enterRecoveryMode(robotId: string): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/api/robots/${robotId}/enter-recovery-mode`, {});
  }

  exitRecoveryMode(robotId: string): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/api/robots/${robotId}/exit-recovery-mode`, {});
  }

  sendTeleopCommand(robotId: string, command: TeleopCommand): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/api/robots/${robotId}/teleop-command`, { command });
  }
}
