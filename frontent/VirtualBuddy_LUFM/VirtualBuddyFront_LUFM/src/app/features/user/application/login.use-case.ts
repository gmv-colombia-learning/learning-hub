import { Observable, tap } from 'rxjs';
import { AuthenticatedUser } from '../domain/authenticated-user';
import { AuthenticationRepository, LoginCredentials } from './authentication.repository';
import { SessionRepository } from './session.repository';

export class LoginUseCase {
  constructor(
    private readonly repository: AuthenticationRepository,
    private readonly session: SessionRepository,
  ) {}

  execute(credentials: LoginCredentials): Observable<AuthenticatedUser> {
    return this.repository.login(credentials).pipe(tap((user) => this.session.save(user)));
  }
}
