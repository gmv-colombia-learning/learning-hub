import { ProjectStatus } from '../domain/project-summary';

const STATUS_LABELS: Record<ProjectStatus, string> = {
  [ProjectStatus.Unknown]: 'Desconocido',
  [ProjectStatus.Active]: 'Activo',
  [ProjectStatus.Inactive]: 'Inactivo',
  [ProjectStatus.Review]: 'En revision',
  [ProjectStatus.Completed]: 'Completado',
};

export function projectStatusLabel(status: ProjectStatus): string {
  return STATUS_LABELS[status] ?? STATUS_LABELS[ProjectStatus.Unknown];
}

export function projectStatusClass(status: ProjectStatus): string {
  return ProjectStatus[status]?.toLowerCase() ?? 'unknown';
}
