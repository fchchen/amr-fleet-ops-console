import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import { Incident } from '../models/fleet.models';

@Injectable({ providedIn: 'root' })
export class IncidentApiService {
  constructor(private readonly http: HttpClient) {}

  getIncidents(): Observable<Incident[]> {
    return this.http.get<Incident[]>(`${API_BASE_URL}/api/incidents`);
  }

  acknowledgeIncident(id: string): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/api/incidents/${id}/acknowledge`, {});
  }
}
