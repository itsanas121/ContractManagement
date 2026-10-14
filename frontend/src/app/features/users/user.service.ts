import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Service } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateUserRequest, UserResponse } from './user.models';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiUrl}/api/users`;

  // GET /api/users
  getAll(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.apiUrl);
  }

  // POST /api/users
  create(request: CreateUserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(
      this.apiUrl,
      request
    );
  }

  // POST /api/users/{id}/activate
  activate(userId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${userId}/activate`,
      null
    );
  }

  // POST /api/users/{id}/deactivate
  deactivate(userId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/${userId}/deactivate`,
      null
    );
  }
}
