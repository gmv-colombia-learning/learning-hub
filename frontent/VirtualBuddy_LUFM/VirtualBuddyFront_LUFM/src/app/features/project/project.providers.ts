import { Provider } from '@angular/core';
import { ListProjectsUseCase } from './application/list-projects.use-case';
import { ProjectRepository } from './application/project.repository';
import { ProjectHttpRepository } from './infrastructure/project.http-repository';
import { PROJECT_REPOSITORY } from './infrastructure/project.tokens';

export const PROJECT_PROVIDERS: Provider[] = [
  ProjectHttpRepository,
  {
    provide: PROJECT_REPOSITORY,
    useExisting: ProjectHttpRepository,
  },
  {
    provide: ListProjectsUseCase,
    useFactory: (repository: ProjectRepository) => new ListProjectsUseCase(repository),
    deps: [PROJECT_REPOSITORY],
  },
];
