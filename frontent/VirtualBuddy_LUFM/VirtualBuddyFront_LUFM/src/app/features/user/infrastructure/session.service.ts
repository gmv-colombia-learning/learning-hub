import { Injectable, signal } from '@angular/core';
import { SessionRepository } from '../application/session.repository';
import { AuthenticatedUser } from '../domain/authenticated-user';

const SESSION_STORAGE_KEY = 'virtual-buddy.session';

@Injectable({ providedIn: 'root' })
export class SessionService implements SessionRepository {
  private readonly currentSession = signal<AuthenticatedUser | null>(this.readStoredSession());

  readonly user = this.currentSession.asReadonly();

  isAuthenticated(): boolean {
    const session = this.currentSession();

    if (!session || !this.hasFutureExpiration(session.token)) {
      this.clear();
      return false;
    }

    return true;
  }

  token(): string | null {
    return this.isAuthenticated() ? (this.currentSession()?.token ?? null) : null;
  }

  save(session: AuthenticatedUser): void {
    if (!this.isValidSession(session)) {
      this.clear();
      return;
    }

    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
    this.currentSession.set(session);
  }

  clear(): void {
    localStorage.removeItem(SESSION_STORAGE_KEY);
    this.currentSession.set(null);
  }

  private readStoredSession(): AuthenticatedUser | null {
    try {
      const value = localStorage.getItem(SESSION_STORAGE_KEY);
      if (!value) return null;

      const session = JSON.parse(value) as Partial<AuthenticatedUser>;
      if (!this.isValidSession(session)) {
        localStorage.removeItem(SESSION_STORAGE_KEY);
        return null;
      }

      return session;
    } catch {
      localStorage.removeItem(SESSION_STORAGE_KEY);
      return null;
    }
  }

  private isValidSession(session: Partial<AuthenticatedUser>): session is AuthenticatedUser {
    return (
      typeof session.token === 'string' &&
      typeof session.email === 'string' &&
      typeof session.fullName === 'string' &&
      this.hasFutureExpiration(session.token)
    );
  }

  private hasFutureExpiration(token: string): boolean {
    try {
      const payload = token.split('.')[1];
      if (!payload) return false;

      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const decoded = JSON.parse(atob(normalized)) as { exp?: unknown };
      return typeof decoded.exp === 'number' && decoded.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }
}
