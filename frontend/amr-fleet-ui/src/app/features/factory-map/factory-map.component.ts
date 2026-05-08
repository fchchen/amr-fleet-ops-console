import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RobotTelemetry } from '../../core/models/fleet.models';

@Component({
  selector: 'app-factory-map',
  imports: [CommonModule],
  templateUrl: './factory-map.component.html',
  styleUrl: './factory-map.component.scss',
})
export class FactoryMapComponent {
  @Input({ required: true }) robots: RobotTelemetry[] = [];
  @Input() selectedRobotId?: string;
  @Output() robotSelected = new EventEmitter<RobotTelemetry>();

  color(robot: RobotTelemetry): string {
    const colors: Record<string, string> = {
      Idle: '#68727c',
      RunningMission: '#1976a3',
      Charging: '#7d4caa',
      Blocked: '#c53939',
      Faulted: '#c53939',
      Offline: '#59616a',
      EmergencyStopped: '#a82020',
      RecoveryMode: '#d66d1f',
    };

    return colors[robot.status] ?? '#68727c';
  }
}
