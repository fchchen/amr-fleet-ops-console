import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { RobotTelemetry } from '../../core/models/fleet.models';
import { RobotApiService } from '../../core/services/robot-api.service';
import { RobotTelemetrySignalRService } from '../../core/signalr/robot-telemetry-signalr.service';
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
export class DashboardComponent implements OnInit, OnDestroy {
  robots: RobotTelemetry[] = [];
  selectedRobot?: RobotTelemetry;
  private readonly subscriptions = new Subscription();

  constructor(
    private readonly robotsApi: RobotApiService,
    private readonly telemetry: RobotTelemetrySignalRService,
  ) {}

  ngOnInit(): void {
    this.telemetry.connect();

    this.subscriptions.add(
      this.telemetry.robots$.subscribe((robots) => {
        this.robots = robots;
        this.selectedRobot =
          robots.find((robot) => robot.robotId === this.selectedRobot?.robotId) ??
          robots.find((robot) => robot.robotId === 'AMR-003') ??
          robots[0];
      }),
    );

    this.refreshRobots();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  selectRobot(robot: RobotTelemetry): void {
    this.selectedRobot = robot;
  }

  refreshRobots(): void {
    this.robotsApi.getRobots().subscribe((robots) => this.telemetry.setSnapshot(robots));
  }
}
