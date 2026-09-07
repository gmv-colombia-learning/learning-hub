import { Observable } from 'rxjs';
import { ProjectSummary } from '../domain/project-summary';

export interface ProjectRepository {
  getAll(): Observable<readonly ProjectSummary[]>;
}
