import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard } from './auth.guard';
import { SessionService } from './session.service';

describe('authGuard', () => {
  it('allows an authenticated user', () => {
    TestBed.configureTestingModule({
      providers: [{ provide: SessionService, useValue: { isAuthenticated: () => true } }],
    });

    const result = runGuard('/projects');

    expect(result).toBe(true);
  });

  it('redirects an anonymous user and preserves the requested URL', () => {
    const loginTree = {} as UrlTree;
    const router = { createUrlTree: vi.fn(() => loginTree) };
    TestBed.configureTestingModule({
      providers: [
        { provide: SessionService, useValue: { isAuthenticated: () => false } },
        { provide: Router, useValue: router },
      ],
    });

    const result = runGuard('/projects/42');

    expect(result).toBe(loginTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/projects/42' },
    });
  });

  function runGuard(url: string): ReturnType<typeof authGuard> {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
  }
});
