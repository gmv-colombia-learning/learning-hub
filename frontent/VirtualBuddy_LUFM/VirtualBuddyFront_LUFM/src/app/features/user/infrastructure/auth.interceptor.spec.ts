import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { authInterceptor } from './auth.interceptor';
import { SessionService } from './session.service';

function validToken(): string {
  const payload = btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 60 }));
  return `header.${payload}.signature`;
}

describe('authInterceptor', () => {
  it('adds the Bearer token only to configured API requests', () => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: 'https://api.example.com' },
      ],
    });
    TestBed.inject(SessionService).save({
      token: validToken(),
      email: 'user@example.com',
      fullName: 'User',
    });
    const client = TestBed.inject(HttpClient);
    const http = TestBed.inject(HttpTestingController);

    client.get('https://api.example.com/projects').subscribe();
    client.get('https://other.example.com/projects').subscribe();

    expect(
      http.expectOne('https://api.example.com/projects').request.headers.get('Authorization'),
    ).toBe(`Bearer ${TestBed.inject(SessionService).token()}`);
    expect(
      http.expectOne('https://other.example.com/projects').request.headers.has('Authorization'),
    ).toBe(false);
    http.verify();
  });
});
