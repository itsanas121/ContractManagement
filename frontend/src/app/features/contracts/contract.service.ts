import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ContractResponse,
  CreateContractRequest,
} from './contract.models';

@Injectable({ providedIn: 'root' })
export class ContractService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/contracts`;

  create(request: CreateContractRequest): Observable<ContractResponse> {
    return this.http.post<ContractResponse>(this.baseUrl, request);
  }
}