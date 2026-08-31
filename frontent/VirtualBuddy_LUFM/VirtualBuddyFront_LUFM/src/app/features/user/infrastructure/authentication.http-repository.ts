import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { AuthenticationError } from '../application/authentication.error';
import {
  AuthenticationRepository,
  LoginCredentials,
} from '../application/authentication.repository';
import { AuthenticatedUser } from '../domain/authenticated-user';
import { AuthResponseDto, LoginRequestDto } from './authentication.dto';

@Injectable()
export class AuthenticationHttpRepository implements AuthenticationRepository {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  login(credentials: LoginCredentials): Observable<AuthenticatedUser> {
    if (!this.apiBaseUrl) {
      return throwError(() => new AuthenticationError('unavailable'));
    }

    const request: LoginRequestDto = credentials;

    return this.http.post<AuthResponseDto>(`${this.apiBaseUrl}/api/Auth/login`, request).pipe(
      map((response) => ({
        token: response.token,
        email: response.email,
        fullName: response.fullName,
      })),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && (error.status === 400 || error.status === 404)) {
          return throwError(() => new AuthenticationError('invalid-credentials'));
        }

        return throwError(() => new AuthenticationError('unavailable'));
      }),
    );
  }
}
