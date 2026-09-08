import { Observable } from 'rxjs';
import { ProjectDetails } from '../domain/project-details';
import { ProjectSummary } from '../domain/project-summary';

export interface ProjectRepository {
  getAll(): Observable<readonly ProjectSummary[]>;
  getById(id: string): Observable<ProjectDetails>;
}
