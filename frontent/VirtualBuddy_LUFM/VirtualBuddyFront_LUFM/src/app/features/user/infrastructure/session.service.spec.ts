import { TestBed } from '@angular/core/testing';
import { SessionService } from './session.service';

function tokenWithExpiration(expiration: number): string {
  const payload = btoa(JSON.stringify({ exp: expiration }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `header.${payload}.signature`;
}

describe('SessionService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.resetTestingModule();
  });

  it('persists and exposes a session with a future expiration', () => {
    const service = TestBed.inject(SessionService);
    const user = {
      token: tokenWithExpiration(Math.floor(Date.now() / 1000) + 60),
      email: 'user@example.com',
      fullName: 'User Name',
    };

    service.save(user);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.user()).toEqual(user);
    expect(localStorage.getItem('virtual-buddy.session')).toContain(user.email);
  });

  it('removes an expired stored session', () => {
    localStorage.setItem(
      'virtual-buddy.session',
      JSON.stringify({
        token: tokenWithExpiration(Math.floor(Date.now() / 1000) - 60),
        email: 'user@example.com',
        fullName: 'User Name',
      }),
    );

    const service = TestBed.inject(SessionService);

    expect(service.isAuthenticated()).toBe(false);
    expect(service.user()).toBeNull();
    expect(localStorage.getItem('virtual-buddy.session')).toBeNull();
  });

  it('removes a malformed stored session', () => {
    localStorage.setItem('virtual-buddy.session', '{invalid');

    const service = TestBed.inject(SessionService);

    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.getItem('virtual-buddy.session')).toBeNull();
  });
});
