import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Health } from './health.model';

@Injectable({ providedIn: 'root' })
export class HealthService {
  private readonly http = inject(HttpClient);

  // Relative URL: the dev proxy sends /api to the API on the same origin.
  getHealth(): Observable<Health> {
    return this.http.get<Health>('/api/health');
  }
}
