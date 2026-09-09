import { Observable } from 'rxjs';

export interface ProjectAssistantRepository {
  ask(projectId: string, question: string): Observable<string>;
}
