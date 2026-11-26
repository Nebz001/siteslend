import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Asset, PagedResult } from './asset.model';

@Injectable({ providedIn: 'root' })
export class AssetService {
  private readonly http = inject(HttpClient);

  // Relative URL: the dev proxy sends /api to the API on the same origin.
  getAssets(page = 1, pageSize = 20): Observable<PagedResult<Asset>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Asset>>('/api/assets', { params });
  }
}
