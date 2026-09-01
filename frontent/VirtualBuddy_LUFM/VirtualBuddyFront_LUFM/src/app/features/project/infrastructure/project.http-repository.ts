import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { ProjectRepository } from '../application/project.repository';
import { ProjectStatus, ProjectSummary } from '../domain/project-summary';
import { ProjectDto } from './project.dto';

@Injectable()
export class ProjectHttpRepository implements ProjectRepository {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL).replace(/\/$/, '');

  getAll(): Observable<readonly ProjectSummary[]> {
    if (!this.apiBaseUrl) {
      return throwError(() => new Error('Project API is unavailable'));
    }

    return this.http
      .get<ProjectDto[]>(`${this.apiBaseUrl}/api/project`)
      .pipe(map((projects) => projects.map((project) => this.toSummary(project))));
  }

  private toSummary(project: ProjectDto): ProjectSummary {
    return {
      id: project.id,
      name: project.name,
      description: project.description,
      status: project.status as ProjectStatus,
      imageUrl: project.urlImage,
    };
  }
}
