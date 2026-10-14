import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateIndividualPartyRequest,
  CreateOrganizationPartyRequest,
  Party,
} from './party.models';

@Injectable({ providedIn: 'root' })
export class PartyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/parties`;

  getAll(): Observable<Party[]> {
    return this.http.get<Party[]>(this.baseUrl);
  }

  registerIndividual(request: CreateIndividualPartyRequest): Observable<Party> {
    return this.http.post<Party>(`${this.baseUrl}/individual`, request);
  }

  registerOrganization(request: CreateOrganizationPartyRequest): Observable<Party> {
    return this.http.post<Party>(`${this.baseUrl}/organization`, request);
  }
}