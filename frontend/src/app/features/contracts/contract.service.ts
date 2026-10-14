import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddContractDocumentRequest,
  AddContractPartyRequest,
  ContractDetailsResponse,
  ContractResponse,
  ContractSummary,
  CreateContractRequest,
} from './contract.models';

@Injectable({ providedIn: 'root' })
export class ContractService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/contracts`;

  create(request: CreateContractRequest): Observable<ContractResponse> {
    return this.http.post<ContractResponse>(this.baseUrl, request);
  }

getById(contractId: number): Observable<ContractDetailsResponse> {
    return this.http.get<ContractDetailsResponse>(
        `${this.baseUrl}/${contractId}`,
  ); 
}

  addParty(
    contractId: number,
    request: AddContractPartyRequest,
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/${contractId}/parties`,
      request,
    );
  }

  addDocument(
    contractId: number,
    request: AddContractDocumentRequest,
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/${contractId}/documents`,
      request,
    );
  }
  getSummary(): Observable<ContractSummary> {
    return this.http.get<ContractSummary>(`${this.baseUrl}/summary`);
  }
}