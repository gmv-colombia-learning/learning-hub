import { Provider } from '@angular/core';
import { AskProjectAssistantUseCase } from './application/ask-project-assistant.use-case';
import { ProjectAssistantRepository } from './application/project-assistant.repository';
import { ProjectAssistantHttpRepository } from './infrastructure/project-assistant.http-repository';
import { PROJECT_ASSISTANT_REPOSITORY } from './infrastructure/project-assistant.tokens';

export const PROJECT_ASSISTANT_PROVIDERS: Provider[] = [
  ProjectAssistantHttpRepository,
  {
    provide: PROJECT_ASSISTANT_REPOSITORY,
    useExisting: ProjectAssistantHttpRepository,
  },
  {
    provide: AskProjectAssistantUseCase,
    useFactory: (repository: ProjectAssistantRepository) =>
      new AskProjectAssistantUseCase(repository),
    deps: [PROJECT_ASSISTANT_REPOSITORY],
  },
];
