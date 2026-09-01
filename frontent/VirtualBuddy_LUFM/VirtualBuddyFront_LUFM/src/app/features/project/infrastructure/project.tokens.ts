import { InjectionToken } from '@angular/core';
import { ProjectRepository } from '../application/project.repository';

export const PROJECT_REPOSITORY = new InjectionToken<ProjectRepository>('PROJECT_REPOSITORY');
