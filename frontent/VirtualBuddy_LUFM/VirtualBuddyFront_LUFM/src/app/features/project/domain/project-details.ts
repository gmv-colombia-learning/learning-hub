import { ProjectStatus } from './project-summary';

export interface ProjectTechnology {
  readonly id: string;
  readonly name: string;
}

export interface ProjectMember {
  readonly userId: string;
  readonly fullName: string;
  readonly role: string;
}

export interface ProjectDetails {
  readonly id: string;
  readonly name: string;
  readonly description: string;
  readonly developmentStartedAt: string;
  readonly status: ProjectStatus;
  readonly architectureInfo: string | null;
  readonly technologies: readonly ProjectTechnology[];
  readonly members: readonly ProjectMember[];
}
