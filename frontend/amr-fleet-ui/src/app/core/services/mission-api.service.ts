import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Mission, MissionCreateRequest } from '../models/fleet.models';
import { API_BASE_URL } from './api-config';

@Injectable({ providedIn: 'root' })
export class MissionApiService {
  constructor(private readonly http: HttpClient) {}

  getMissions(): Observable<Mission[]> {
    return this.http.get<Mission[]>(`${API_BASE_URL}/api/missions`);
  }

  createMission(request: MissionCreateRequest): Observable<Mission> {
    return this.http.post<Mission>(`${API_BASE_URL}/api/missions`, request);
  }
}
