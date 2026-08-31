import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { SessionService } from './session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');
  const token = inject(SessionService).token();
  const targetsApi =
    !!apiBaseUrl && (request.url === apiBaseUrl || request.url.startsWith(`${apiBaseUrl}/`));

  if (!token || !targetsApi) return next(request);

  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
