export interface ProjectTechnologyDto {
  id: string;
  name: string;
}

export interface ProjectMemberDto {
  userId: string;
  fullName: string;
  role: string;
}

export interface ProjectDto {
  id: string;
  name: string;
  acronym: string | null;
  description: string;
  developmentTime: string;
  status: 0 | 1 | 2 | 3 | 4;
  urlImage: string;
  architectureInfo: string | null;
  technologies: ProjectTechnologyDto[];
  members: ProjectMemberDto[];
}
