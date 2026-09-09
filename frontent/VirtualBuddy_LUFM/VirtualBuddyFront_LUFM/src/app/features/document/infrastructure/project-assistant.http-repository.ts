import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { ProjectAssistantRepository } from '../application/project-assistant.repository';
import { ProjectAssistantResponseDto } from './project-assistant.dto';

@Injectable()
export class ProjectAssistantHttpRepository implements ProjectAssistantRepository {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  ask(projectId: string, question: string): Observable<string> {
    if (!this.apiBaseUrl) {
      return throwError(() => new Error('Project assistant API is unavailable'));
    }

    return this.http
      .post<ProjectAssistantResponseDto>(`${this.apiBaseUrl}/api/AI/chat/${projectId}`, {
        question,
      })
      .pipe(map(({ response }) => response));
  }
}
