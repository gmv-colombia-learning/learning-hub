export interface AuthenticatedUser {
  readonly token: string;
  readonly email: string;
  readonly fullName: string;
}
