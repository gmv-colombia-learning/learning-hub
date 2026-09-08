import { of } from 'rxjs';
import { ProjectDetails } from '../domain/project-details';
import { ProjectStatus } from '../domain/project-summary';
import { GetProjectDetailsUseCase } from './get-project-details.use-case';
import { ProjectRepository } from './project.repository';

describe('GetProjectDetailsUseCase', () => {
  it('requests the project identified by the route', () => {
    const project = projectDetails();
    const repository: ProjectRepository = {
      getAll: vi.fn(() => of([])),
      getById: vi.fn(() => of(project)),
    };
    const useCase = new GetProjectDetailsUseCase(repository);
    let result: ProjectDetails | undefined;

    useCase.execute('project-1').subscribe((value) => (result = value));

    expect(repository.getById).toHaveBeenCalledWith('project-1');
    expect(result).toBe(project);
  });

  function projectDetails(): ProjectDetails {
    return {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Mentoria virtual',
      developmentStartedAt: '2026-08-01T00:00:00Z',
      status: ProjectStatus.Active,
      architectureInfo: null,
      technologies: [],
      members: [],
    };
  }
});
