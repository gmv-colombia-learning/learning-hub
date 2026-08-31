import { AuthenticatedUser } from '../domain/authenticated-user';

export interface SessionRepository {
  save(session: AuthenticatedUser): void;
}
