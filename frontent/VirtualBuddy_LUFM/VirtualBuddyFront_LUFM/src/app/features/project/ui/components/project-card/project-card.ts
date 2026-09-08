import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectSummary } from '../../../domain/project-summary';
import { projectStatusClass, projectStatusLabel } from '../../project-status.presentation';

@Component({
  selector: 'app-project-card',
  imports: [RouterLink],
  templateUrl: './project-card.html',
  styleUrl: './project-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectCard {
  readonly project = input.required<ProjectSummary>();
  protected readonly fallbackImageUrl = '/sin-imagen.png';
  protected readonly statusLabel = projectStatusLabel;
  protected readonly statusClass = projectStatusClass;

  protected showFallbackImage(event: Event): void {
    const image = event.target as HTMLImageElement;

    if (!image.src.endsWith(this.fallbackImageUrl)) {
      image.src = this.fallbackImageUrl;
      image.classList.add('project-image--fallback');
    }
  }
}
