import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";

import { environment } from "../../../environments/environment";
import { CompanyResponse, CreateCompanyRequest } from "./company.models";


@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/api/companies`;

  getAll(): Observable<CompanyResponse[]> {
    return this.http.get<CompanyResponse[]>(this.apiUrl);
  }

  create(request: CreateCompanyRequest): Observable<CompanyResponse> {
    return this.http.post<CompanyResponse>(
      this.apiUrl,
      request
    );
  }
}
