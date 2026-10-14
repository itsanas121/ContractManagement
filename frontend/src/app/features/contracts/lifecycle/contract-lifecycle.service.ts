import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

// Calls the contract lifecycle endpoints (ContractLifecycleController).
// The auth interceptor adds the JWT automatically.
@Injectable({ providedIn: 'root' })
export class ContractLifecycleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/contracts`;

  submit(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/submit`, null);
  }

  approve(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, null);
  }

  reject(id: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/reject`, { reason });
  }

  activate(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/activate`, null);
  }

  terminate(id: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/terminate`, { reason });
  }
}