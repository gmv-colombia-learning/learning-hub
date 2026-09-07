import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ProjectStatus, ProjectSummary } from '../../../domain/project-summary';

const STATUS_LABELS: Record<ProjectStatus, string> = {
  [ProjectStatus.Unknown]: 'Desconocido',
  [ProjectStatus.Active]: 'Activo',
  [ProjectStatus.Inactive]: 'Inactivo',
  [ProjectStatus.Review]: 'En revision',
  [ProjectStatus.Completed]: 'Completado',
};

@Component({
  selector: 'app-project-card',
  templateUrl: './project-card.html',
  styleUrl: './project-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectCard {
  readonly project = input.required<ProjectSummary>();
  protected readonly fallbackImageUrl = '/sin-imagen.png';

  protected showFallbackImage(event: Event): void {
    const image = event.target as HTMLImageElement;

    if (!image.src.endsWith(this.fallbackImageUrl)) {
      image.src = this.fallbackImageUrl;
      image.classList.add('project-image--fallback');
    }
  }

  protected statusLabel(status: ProjectStatus): string {
    return STATUS_LABELS[status] ?? STATUS_LABELS[ProjectStatus.Unknown];
  }

  protected statusClass(status: ProjectStatus): string {
    return ProjectStatus[status]?.toLowerCase() ?? 'unknown';
  }
}
