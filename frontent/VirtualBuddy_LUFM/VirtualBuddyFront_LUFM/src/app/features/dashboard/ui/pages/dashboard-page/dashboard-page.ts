import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { ListProjectsUseCase } from '../../../../project/application/list-projects.use-case';
import { ProjectSummary } from '../../../../project/domain/project-summary';
import { ProjectCard } from '../../../../project/ui/components/project-card/project-card';

type ProjectListState =
  | { readonly status: 'loading' }
  | { readonly status: 'success'; readonly projects: readonly ProjectSummary[] }
  | { readonly status: 'error' };

@Component({
  selector: 'app-dashboard-page',
  imports: [ProjectCard],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {
  private readonly listProjects = inject(ListProjectsUseCase);
  private readonly retryRequest = signal(0);

  protected readonly state = toSignal(
    toObservable(this.retryRequest).pipe(
      switchMap(() =>
        this.listProjects.execute().pipe(
          map((projects) => ({ status: 'success', projects }) as const),
          startWith({ status: 'loading' } as const),
          catchError(() => of({ status: 'error' } as const)),
        ),
      ),
    ),
    { initialValue: { status: 'loading' } as ProjectListState },
  );

  protected retry(): void {
    this.retryRequest.update((request) => request + 1);
  }
}
