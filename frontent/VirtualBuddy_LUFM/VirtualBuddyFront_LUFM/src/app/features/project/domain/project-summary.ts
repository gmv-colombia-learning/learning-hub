export enum ProjectStatus {
  Unknown = 0,
  Active = 1,
  Inactive = 2,
  Review = 3,
  Completed = 4,
}

export interface ProjectSummary {
  readonly id: string;
  readonly name: string;
  readonly description: string;
  readonly status: ProjectStatus;
  readonly imageUrl: string;
}
