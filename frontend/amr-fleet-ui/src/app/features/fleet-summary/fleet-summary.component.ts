import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RobotTelemetry } from '../../core/models/fleet.models';

@Component({
  selector: 'app-fleet-summary',
  imports: [CommonModule],
  templateUrl: './fleet-summary.component.html',
  styleUrl: './fleet-summary.component.scss',
})
export class FleetSummaryComponent {
  @Input({ required: true }) robots: RobotTelemetry[] = [];
  @Input() selectedRobotId?: string;
  @Output() robotSelected = new EventEmitter<RobotTelemetry>();
}
