import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { ProjectNotFoundError } from '../application/project-query.error';
import { ProjectRepository } from '../application/project.repository';
import { ProjectDetails } from '../domain/project-details';
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

  getById(id: string): Observable<ProjectDetails> {
    if (!this.apiBaseUrl) {
      return throwError(() => new Error('Project API is unavailable'));
    }

    return this.http.get<ProjectDto>(`${this.apiBaseUrl}/api/project/${id}`).pipe(
      map((project) => this.toDetails(project)),
      catchError((error: unknown) =>
        throwError(() =>
          error instanceof HttpErrorResponse && error.status === 404
            ? new ProjectNotFoundError()
            : error,
        ),
      ),
    );
  }

  private toSummary(project: ProjectDto): ProjectSummary {
    return {
      id: project.id,
      name: project.name,
      description: project.description,
      status: project.status as ProjectStatus,
      imageUrl: project.urlImage ?? '',
    };
  }

  private toDetails(project: ProjectDto): ProjectDetails {
    return {
      id: project.id,
      name: project.name,
      description: project.description,
      developmentStartedAt: project.developmentTime,
      status: project.status as ProjectStatus,
      architectureInfo: project.architectureInfo,
      technologies: project.technologies.map((technology) => ({ ...technology })),
      members: project.members.map((member) => ({ ...member })),
    };
  }
}
