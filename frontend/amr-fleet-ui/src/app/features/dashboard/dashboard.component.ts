import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { take } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RobotTelemetry } from '../../core/models/fleet.models';
import { RobotApiService } from '../../core/services/robot-api.service';
import {
  RobotTelemetrySignalRService,
  TelemetryConnectionState,
} from '../../core/signalr/robot-telemetry-signalr.service';
import { DiagnosticsComponent } from '../diagnostics/diagnostics.component';
import { FactoryMapComponent } from '../factory-map/factory-map.component';
import { FleetSummaryComponent } from '../fleet-summary/fleet-summary.component';
import { IncidentsComponent } from '../incidents/incidents.component';
import { MissionPlannerComponent } from '../mission-planner/mission-planner.component';
import { RecoveryControlsComponent } from '../recovery/recovery-controls.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    FleetSummaryComponent,
    FactoryMapComponent,
    MissionPlannerComponent,
    IncidentsComponent,
    DiagnosticsComponent,
    RecoveryControlsComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  robots: RobotTelemetry[] = [];
  selectedRobot?: RobotTelemetry;
  connectionState: TelemetryConnectionState = 'idle';
  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private readonly robotsApi: RobotApiService,
    private readonly telemetry: RobotTelemetrySignalRService,
  ) {}

  ngOnInit(): void {
    this.telemetry.connect();

    this.telemetry.robots$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((robots) => {
        this.robots = robots;
        this.selectedRobot = robots.find((robot) => robot.robotId === this.selectedRobot?.robotId) ?? robots[0];
      });

    this.telemetry.connectionState$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((state) => {
        this.connectionState = state;
      });

    this.refreshRobots();
  }

  selectRobot(robot: RobotTelemetry): void {
    this.selectedRobot = robot;
  }

  refreshRobots(): void {
    this.robotsApi
      .getRobots()
      .pipe(take(1))
      .subscribe((robots) => this.telemetry.setSnapshot(robots));
  }
}
