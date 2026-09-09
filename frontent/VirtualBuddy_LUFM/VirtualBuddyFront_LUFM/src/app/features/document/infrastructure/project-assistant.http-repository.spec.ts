import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { ProjectAssistantHttpRepository } from './project-assistant.http-repository';

describe('ProjectAssistantHttpRepository', () => {
  let repository: ProjectAssistantHttpRepository;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ProjectAssistantHttpRepository,
        { provide: API_BASE_URL, useValue: 'https://localhost:5001/' },
      ],
    });
    repository = TestBed.inject(ProjectAssistantHttpRepository);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts the question and maps the verified response contract', () => {
    let result: string | undefined;
    repository.ask('project-1', 'What is the architecture?').subscribe((value) => (result = value));

    const request = http.expectOne('https://localhost:5001/api/AI/chat/project-1');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ question: 'What is the architecture?' });
    request.flush({ response: 'Clean Architecture' });

    expect(result).toBe('Clean Architecture');
  });

  it('propagates assistant errors', () => {
    let result: unknown;
    repository.ask('project-1', 'Question').subscribe({
      error: (error: unknown) => (result = error),
    });

    http
      .expectOne('https://localhost:5001/api/AI/chat/project-1')
      .flush({}, { status: 503, statusText: 'Unavailable' });

    expect(result).toBeInstanceOf(HttpErrorResponse);
    expect((result as HttpErrorResponse).status).toBe(503);
  });
});
