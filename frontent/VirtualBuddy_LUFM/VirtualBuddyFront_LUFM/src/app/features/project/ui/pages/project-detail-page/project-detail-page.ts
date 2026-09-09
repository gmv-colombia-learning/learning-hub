import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { MatButton } from '@angular/material/button';
import { MatTab, MatTabGroup } from '@angular/material/tabs';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { ProjectAssistantChat } from '../../../../document/project-assistant-chat';
import { GetProjectDetailsUseCase } from '../../../application/get-project-details.use-case';
import { ProjectNotFoundError } from '../../../application/project-query.error';
import { formatDevelopmentDuration } from '../../../domain/development-duration';
import { ProjectDetails } from '../../../domain/project-details';
import { ProjectAiSummary } from '../../components/project-ai-summary/project-ai-summary';
import { projectStatusClass, projectStatusLabel } from '../../project-status.presentation';

type ProjectDetailState =
  | { readonly status: 'loading' }
  | { readonly status: 'success'; readonly project: ProjectDetails }
  | { readonly status: 'not-found' }
  | { readonly status: 'error' };

@Component({
  selector: 'app-project-detail-page',
  imports: [MatButton, MatTab, MatTabGroup, ProjectAiSummary, ProjectAssistantChat, RouterLink],
  templateUrl: './project-detail-page.html',
  styleUrl: './project-detail-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailPage {
  private readonly getProjectDetails = inject(GetProjectDetailsUseCase);
  private readonly route = inject(ActivatedRoute);
  private readonly retryRequest = signal(0);

  protected readonly statusLabel = projectStatusLabel;
  protected readonly statusClass = projectStatusClass;
  protected readonly developmentDuration = formatDevelopmentDuration;

  protected readonly state = toSignal(
    combineLatest([
      this.route.paramMap.pipe(map((params) => params.get('id') ?? '')),
      toObservable(this.retryRequest),
    ]).pipe(
      switchMap(([id]) =>
        this.getProjectDetails.execute(id).pipe(
          map((project) => ({ status: 'success', project }) as const),
          startWith({ status: 'loading' } as const),
          catchError((error: unknown) =>
            of(
              error instanceof ProjectNotFoundError
                ? ({ status: 'not-found' } as const)
                : ({ status: 'error' } as const),
            ),
          ),
        ),
      ),
    ),
    { initialValue: { status: 'loading' } as ProjectDetailState },
  );

  protected retry(): void {
    this.retryRequest.update((request) => request + 1);
  }

  protected memberInitials(fullName: string): string {
    const names = fullName.trim().split(/\s+/).filter(Boolean);
    if (names.length === 0) return '?';

    const initials = names.length === 1 ? names[0][0] : names[0][0] + names.at(-1)![0];
    return initials.toUpperCase();
  }

  protected hasArchitecture(project: ProjectDetails): boolean {
    return Boolean(project.architectureInfo?.trim());
  }
}
