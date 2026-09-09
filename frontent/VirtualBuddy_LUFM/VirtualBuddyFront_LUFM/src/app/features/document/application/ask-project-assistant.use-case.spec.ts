import { of } from 'rxjs';
import { ProjectAssistantRepository } from './project-assistant.repository';
import { AskProjectAssistantUseCase } from './ask-project-assistant.use-case';

describe('AskProjectAssistantUseCase', () => {
  it('delegates the project and question to the repository', () => {
    const repository: ProjectAssistantRepository = {
      ask: vi.fn(() => of('Assistant response')),
    };
    const useCase = new AskProjectAssistantUseCase(repository);

    let result: string | undefined;
    useCase.execute('project-1', 'What is this project?').subscribe((value) => (result = value));

    expect(repository.ask).toHaveBeenCalledWith('project-1', 'What is this project?');
    expect(result).toBe('Assistant response');
  });
});
