import { of } from 'rxjs';
import { ProjectStatus } from '../domain/project-summary';
import { ListProjectsUseCase } from './list-projects.use-case';
import { ProjectRepository } from './project.repository';

describe('ListProjectsUseCase', () => {
  it('returns every project in repository order', () => {
    const projects = [
      {
        id: 'project-2',
        name: 'Second',
        description: 'Second project',
        status: ProjectStatus.Completed,
        imageUrl: 'second.png',
      },
      {
        id: 'project-1',
        name: 'First',
        description: 'First project',
        status: ProjectStatus.Inactive,
        imageUrl: 'first.png',
      },
    ];
    const repository: ProjectRepository = { getAll: vi.fn(() => of(projects)) };
    let result: unknown;

    new ListProjectsUseCase(repository).execute().subscribe((value) => (result = value));

    expect(repository.getAll).toHaveBeenCalledOnce();
    expect(result).toBe(projects);
  });
});
