import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { RobotTelemetry } from '../models/fleet.models';
import { API_BASE_URL } from '../services/api-config';

@Injectable({ providedIn: 'root' })
export class RobotTelemetrySignalRService {
  private readonly robotsSubject = new BehaviorSubject<RobotTelemetry[]>([]);
  private connection?: signalR.HubConnection;

  readonly robots$ = this.robotsSubject.asObservable();

  connect(): void {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/robotTelemetry`)
      .withAutomaticReconnect()
      .build();

    this.connection.on('robotTelemetryUpdated', (robots: RobotTelemetry[]) => {
      this.robotsSubject.next(robots);
    });

    void this.connection.start().catch((error) => {
      console.error('SignalR connection failed', error);
    });
  }

  setSnapshot(robots: RobotTelemetry[]): void {
    this.robotsSubject.next(robots);
  }
}
