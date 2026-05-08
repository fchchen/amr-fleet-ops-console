import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RobotTelemetry } from '../../core/models/fleet.models';
import { MissionApiService } from '../../core/services/mission-api.service';

@Component({
  selector: 'app-mission-planner',
  imports: [CommonModule, FormsModule],
  templateUrl: './mission-planner.component.html',
  styleUrl: './mission-planner.component.scss',
})
export class MissionPlannerComponent {
  @Input({ required: true }) robots: RobotTelemetry[] = [];
  @Output() missionCreated = new EventEmitter<void>();

  readonly points = [
    'Line A',
    'Line B',
    'Warehouse Zone 1',
    'Warehouse Zone 2',
    'Charging Station',
    'Inspection Bay',
  ];
  readonly priorities = ['Normal', 'High', 'Critical'];

  robotId = '';
  pickupPoint = 'Line A';
  dropoffPoint = 'Inspection Bay';
  priority = 'Normal';
  error = '';

  constructor(private readonly missions: MissionApiService) {}

  get idleRobots(): RobotTelemetry[] {
    return this.robots.filter((robot) => robot.status === 'Idle');
  }

  assignMission(): void {
    this.error = '';
    if (!this.robotId) {
      this.error = 'Select an idle robot before assigning a mission.';
      return;
    }

    this.missions
      .createMission({
        robotId: this.robotId,
        pickupPoint: this.pickupPoint,
        dropoffPoint: this.dropoffPoint,
        priority: this.priority,
      })
      .subscribe({
        next: () => {
          this.robotId = '';
          this.missionCreated.emit();
        },
        error: (error) => {
          this.error = error?.error?.message ?? 'Mission assignment failed.';
        },
      });
  }
}
