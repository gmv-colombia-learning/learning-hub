import { Observable } from 'rxjs';
import { AuthenticatedUser } from '../domain/authenticated-user';

export interface LoginCredentials {
  readonly email: string;
  readonly password: string;
}

export interface AuthenticationRepository {
  login(credentials: LoginCredentials): Observable<AuthenticatedUser>;
}
