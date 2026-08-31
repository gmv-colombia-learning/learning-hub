import { InjectionToken } from '@angular/core';
import { AuthenticationRepository } from '../application/authentication.repository';

export const AUTHENTICATION_REPOSITORY = new InjectionToken<AuthenticationRepository>(
  'AUTHENTICATION_REPOSITORY',
);
