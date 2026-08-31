import { Routes } from '@angular/router';
import { LoginUseCase } from './application/login.use-case';
import { AuthenticationHttpRepository } from './infrastructure/authentication.http-repository';
import { anonymousGuard } from './infrastructure/auth.guard';
import { SessionService } from './infrastructure/session.service';
import { AUTHENTICATION_REPOSITORY } from './infrastructure/user.tokens';

export const USER_ROUTES: Routes = [
  {
    path: '',
    canActivate: [anonymousGuard],
    providers: [
      AuthenticationHttpRepository,
      {
        provide: AUTHENTICATION_REPOSITORY,
        useExisting: AuthenticationHttpRepository,
      },
      {
        provide: LoginUseCase,
        useFactory: (repository: AuthenticationHttpRepository, session: SessionService) =>
          new LoginUseCase(repository, session),
        deps: [AUTHENTICATION_REPOSITORY, SessionService],
      },
    ],
    loadComponent: () =>
      import('./ui/pages/login-page/login-page').then((component) => component.LoginPage),
  },
];
