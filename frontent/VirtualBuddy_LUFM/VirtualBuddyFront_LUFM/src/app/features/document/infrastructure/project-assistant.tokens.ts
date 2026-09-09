import { InjectionToken } from '@angular/core';
import { ProjectAssistantRepository } from '../application/project-assistant.repository';

export const PROJECT_ASSISTANT_REPOSITORY = new InjectionToken<ProjectAssistantRepository>(
  'PROJECT_ASSISTANT_REPOSITORY',
);
