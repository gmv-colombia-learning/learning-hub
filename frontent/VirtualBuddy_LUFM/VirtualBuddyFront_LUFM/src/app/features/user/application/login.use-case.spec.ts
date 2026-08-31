import { of } from 'rxjs';
import { AuthenticationRepository } from './authentication.repository';
import { LoginUseCase } from './login.use-case';
import { SessionRepository } from './session.repository';

describe('LoginUseCase', () => {
  it('persists the authenticated user returned by the repository', () => {
    const user = { token: 'token', email: 'user@example.com', fullName: 'User Name' };
    const repository: AuthenticationRepository = { login: vi.fn(() => of(user)) };
    const session: SessionRepository = { save: vi.fn() };

    new LoginUseCase(repository, session)
      .execute({ email: user.email, password: 'Password1' })
      .subscribe();

    expect(repository.login).toHaveBeenCalledWith({
      email: 'user@example.com',
      password: 'Password1',
    });
    expect(session.save).toHaveBeenCalledWith(user);
  });
});
