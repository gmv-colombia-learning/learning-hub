import { Observable } from 'rxjs';
import { ProjectDetails } from '../domain/project-details';
import { ProjectRepository } from './project.repository';

export class GetProjectDetailsUseCase {
  constructor(private readonly repository: ProjectRepository) {}

  execute(id: string): Observable<ProjectDetails> {
    return this.repository.getById(id);
  }
}
