import { Observable } from 'rxjs';
import { ProjectSummary } from '../domain/project-summary';
import { ProjectRepository } from './project.repository';

export class ListProjectsUseCase {
  constructor(private readonly repository: ProjectRepository) {}

  execute(): Observable<readonly ProjectSummary[]> {
    return this.repository.getAll();
  }
}
