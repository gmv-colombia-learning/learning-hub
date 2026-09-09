import { Observable } from 'rxjs';
import { ProjectAssistantRepository } from './project-assistant.repository';

export class AskProjectAssistantUseCase {
  constructor(private readonly repository: ProjectAssistantRepository) {}

  execute(projectId: string, question: string): Observable<string> {
    return this.repository.ask(projectId, question);
  }
}
