import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Incident } from '../../core/models/fleet.models';
import { IncidentApiService } from '../../core/services/incident-api.service';

@Component({
  selector: 'app-incidents',
  imports: [CommonModule],
  templateUrl: './incidents.component.html',
  styleUrl: './incidents.component.scss',
})
export class IncidentsComponent implements OnInit {
  @Output() incidentChanged = new EventEmitter<void>();

  incidents: Incident[] = [];

  constructor(private readonly incidentApi: IncidentApiService) {}

  ngOnInit(): void {
    this.loadIncidents();
  }

  loadIncidents(): void {
    this.incidentApi.getIncidents().subscribe((incidents) => {
      this.incidents = incidents.filter((incident) => incident.status !== 'Resolved');
    });
  }

  acknowledge(incident: Incident): void {
    this.incidentApi.acknowledgeIncident(incident.id).subscribe(() => {
      this.loadIncidents();
      this.incidentChanged.emit();
    });
  }
}
