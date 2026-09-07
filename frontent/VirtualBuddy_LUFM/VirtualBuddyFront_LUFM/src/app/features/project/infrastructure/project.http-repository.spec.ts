import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/config/api-base-url.token';
import { ProjectStatus } from '../domain/project-summary';
import { ProjectDto } from './project.dto';
import { ProjectHttpRepository } from './project.http-repository';

describe('ProjectHttpRepository', () => {
  let repository: ProjectHttpRepository;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ProjectHttpRepository,
        { provide: API_BASE_URL, useValue: 'https://localhost:5001/' },
      ],
    });
    repository = TestBed.inject(ProjectHttpRepository);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests all projects and maps only the summary fields', () => {
    let result: unknown;
    repository.getAll().subscribe((projects) => (result = projects));

    const request = http.expectOne('https://localhost:5001/api/project');
    expect(request.request.method).toBe('GET');
    request.flush([projectDto()]);

    expect(result).toEqual([
      {
        id: 'project-1',
        name: 'Virtual Buddy',
        description: 'Mentoria virtual',
        status: ProjectStatus.Review,
        imageUrl: 'https://images.example/project.png',
      },
    ]);
  });

  it('propagates HTTP errors to the presentation flow', () => {
    let result: unknown;
    repository.getAll().subscribe({ error: (error: unknown) => (result = error) });

    http
      .expectOne('https://localhost:5001/api/project')
      .flush({}, { status: 500, statusText: 'Server Error' });

    expect(result).toBeTruthy();
  });

  function projectDto(): ProjectDto {
    return {
      id: 'project-1',
      name: 'Virtual Buddy',
      acronym: 'VB',
      description: 'Mentoria virtual',
      developmentTime: '2026-08-31T12:00:00Z',
      status: 3,
      urlImage: 'https://images.example/project.png',
      architectureInfo: null,
      technologies: [{ id: 'technology-1', name: 'Angular' }],
      members: [{ userId: 'user-1', fullName: 'User Name', role: 'Developer' }],
    };
  }
});
