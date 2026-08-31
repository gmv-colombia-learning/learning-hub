import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { AuthenticationError } from '../application/authentication.error';
import { AuthenticationHttpRepository } from './authentication.http-repository';

describe('AuthenticationHttpRepository', () => {
  let repository: AuthenticationHttpRepository;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthenticationHttpRepository,
        { provide: API_BASE_URL, useValue: 'https://localhost:5001' },
      ],
    });
    repository = TestBed.inject(AuthenticationHttpRepository);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('maps the verified login contract', () => {
    let result: unknown;
    repository
      .login({ email: 'user@example.com', password: 'Password1' })
      .subscribe((value) => (result = value));

    const request = http.expectOne('https://localhost:5001/api/Auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ email: 'user@example.com', password: 'Password1' });
    request.flush({ token: 'jwt', email: 'user@example.com', fullName: 'User Name' });

    expect(result).toEqual({ token: 'jwt', email: 'user@example.com', fullName: 'User Name' });
  });

  it.each([400, 404])('maps status %i to invalid credentials', (status) => {
    let result: unknown;
    repository.login({ email: 'user@example.com', password: 'wrong' }).subscribe({
      error: (error: unknown) => (result = error),
    });

    http
      .expectOne('https://localhost:5001/api/Auth/login')
      .flush({}, { status, statusText: 'Error' });

    expect(result).toBeInstanceOf(AuthenticationError);
    expect((result as AuthenticationError).kind).toBe('invalid-credentials');
  });
});
