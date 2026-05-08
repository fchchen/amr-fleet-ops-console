import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RobotTelemetry } from '../../core/models/fleet.models';

@Component({
  selector: 'app-diagnostics',
  imports: [CommonModule],
  templateUrl: './diagnostics.component.html',
  styleUrl: './diagnostics.component.scss',
})
export class DiagnosticsComponent {
  @Input() robot?: RobotTelemetry;
}
