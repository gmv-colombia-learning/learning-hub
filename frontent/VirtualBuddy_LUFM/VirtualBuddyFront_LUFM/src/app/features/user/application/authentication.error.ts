export type AuthenticationErrorKind = 'invalid-credentials' | 'unavailable';

export class AuthenticationError extends Error {
  constructor(readonly kind: AuthenticationErrorKind) {
    super(kind);
    this.name = 'AuthenticationError';
  }
}
