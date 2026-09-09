import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { AskProjectAssistantUseCase } from '../../../application/ask-project-assistant.use-case';
import { ProjectAssistantChat } from './project-assistant-chat';

describe('ProjectAssistantChat', () => {
  let fixture: ComponentFixture<ProjectAssistantChat>;
  let askProjectAssistant: { execute: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    askProjectAssistant = { execute: vi.fn(() => of('Assistant response')) };

    await TestBed.configureTestingModule({
      imports: [ProjectAssistantChat],
      providers: [{ provide: AskProjectAssistantUseCase, useValue: askProjectAssistant }],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectAssistantChat);
    fixture.componentRef.setInput('projectId', 'project-1');
    fixture.detectChanges();
  });

  it('does not query the assistant before a valid question is submitted', () => {
    expect(askProjectAssistant.execute).not.toHaveBeenCalled();
    expect(sendButton().disabled).toBe(true);

    enterQuestion('   ');

    expect(sendButton().disabled).toBe(true);
    expect(askProjectAssistant.execute).not.toHaveBeenCalled();
  });

  it('trims a question, blocks concurrent submissions and renders the response', () => {
    const response = new Subject<string>();
    askProjectAssistant.execute.mockReturnValue(response);
    enterQuestion('  What is the architecture?  ');

    submitForm();
    fixture.detectChanges();

    expect(askProjectAssistant.execute).toHaveBeenCalledOnce();
    expect(askProjectAssistant.execute).toHaveBeenCalledWith(
      'project-1',
      'What is the architecture?',
    );
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain(
      'Consultando...',
    );
    expect(sendButton().disabled).toBe(true);

    response.next('Clean Architecture\nwith Angular.');
    response.complete();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.assistant-response').textContent).toContain(
      'Clean Architecture\nwith Angular.',
    );
  });

  it('keeps multiple questions and responses in session order', () => {
    askProjectAssistant.execute
      .mockReturnValueOnce(of('First response'))
      .mockReturnValueOnce(of('Second response'));

    enterQuestion('First question');
    submitForm();
    fixture.detectChanges();
    enterQuestion('Second question');
    submitForm();
    fixture.detectChanges();

    const turns = fixture.nativeElement.querySelectorAll('.turn');
    expect(turns).toHaveLength(2);
    expect(turns[0].textContent).toContain('First question');
    expect(turns[0].textContent).toContain('First response');
    expect(turns[1].textContent).toContain('Second question');
    expect(turns[1].textContent).toContain('Second response');
  });

  it('retries a failed question without duplicating it', () => {
    askProjectAssistant.execute
      .mockReturnValueOnce(throwError(() => new Error('Unavailable')))
      .mockReturnValueOnce(of('Recovered response'));

    enterQuestion('Failed question');
    submitForm();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'No fue posible obtener una respuesta.',
    );

    fixture.nativeElement.querySelector('.message--error button').click();
    fixture.detectChanges();

    expect(askProjectAssistant.execute).toHaveBeenCalledTimes(2);
    expect(askProjectAssistant.execute).toHaveBeenLastCalledWith('project-1', 'Failed question');
    expect(fixture.nativeElement.querySelectorAll('.turn')).toHaveLength(1);
    expect(fixture.nativeElement.querySelector('.assistant-response').textContent).toContain(
      'Recovered response',
    );
  });

  function enterQuestion(question: string): void {
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = question;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function sendButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('button[type="submit"]');
  }

  function submitForm(): void {
    const event = new SubmitEvent('submit', { bubbles: true, cancelable: true });
    fixture.nativeElement.querySelector('form').dispatchEvent(event);
    expect(event.defaultPrevented).toBe(true);
  }
});
