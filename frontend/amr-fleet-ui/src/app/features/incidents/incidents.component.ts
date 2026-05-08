import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
  loading = false;
  acknowledgingIncidentId = '';
  private readonly destroyRef = inject(DestroyRef);

  constructor(private readonly incidentApi: IncidentApiService) {}

  ngOnInit(): void {
    this.loadIncidents();
  }

  loadIncidents(): void {
    this.loading = true;
    this.incidentApi
      .getIncidents()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loading = false;
        }),
      )
      .subscribe((incidents) => {
        this.incidents = incidents.filter((incident) => incident.status !== 'Resolved');
      });
  }

  acknowledge(incident: Incident): void {
    this.acknowledgingIncidentId = incident.id;
    this.incidentApi
      .acknowledgeIncident(incident.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.acknowledgingIncidentId = '';
        }),
      )
      .subscribe(() => {
        this.loadIncidents();
        this.incidentChanged.emit();
      });
  }
}
