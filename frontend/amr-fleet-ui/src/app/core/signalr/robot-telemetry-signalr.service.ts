import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { RobotTelemetry } from '../models/fleet.models';
import { API_BASE_URL } from '../services/api-config';

export type TelemetryConnectionState = 'idle' | 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

@Injectable({ providedIn: 'root' })
export class RobotTelemetrySignalRService {
  private readonly robotsSubject = new BehaviorSubject<RobotTelemetry[]>([]);
  private readonly connectionStateSubject = new BehaviorSubject<TelemetryConnectionState>('idle');
  private connection?: signalR.HubConnection;

  readonly robots$ = this.robotsSubject.asObservable();
  readonly connectionState$ = this.connectionStateSubject.asObservable();

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

    this.connection.onreconnecting(() => {
      this.connectionStateSubject.next('reconnecting');
    });

    this.connection.onreconnected(() => {
      this.connectionStateSubject.next('connected');
    });

    this.connection.onclose(() => {
      this.connectionStateSubject.next('disconnected');
    });

    this.connectionStateSubject.next('connecting');
    void this.connection.start().catch((error) => {
      console.error('SignalR connection failed', error);
      this.connectionStateSubject.next('disconnected');
    }).then(() => {
      if (this.connection?.state === signalR.HubConnectionState.Connected) {
        this.connectionStateSubject.next('connected');
      }
    });
  }

  setSnapshot(robots: RobotTelemetry[]): void {
    this.robotsSubject.next(robots);
  }
}
