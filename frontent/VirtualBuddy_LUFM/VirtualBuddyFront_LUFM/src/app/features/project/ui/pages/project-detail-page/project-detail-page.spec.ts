import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';
import { GetProjectDetailsUseCase } from '../../../application/get-project-details.use-case';
import { ProjectNotFoundError } from '../../../application/project-query.error';
import { ProjectDetails } from '../../../domain/project-details';
import { ProjectStatus } from '../../../domain/project-summary';
import { ProjectDetailPage } from './project-detail-page';

describe('ProjectDetailPage', () => {
  let fixture: ComponentFixture<ProjectDetailPage>;
  let getProjectDetails: { execute: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    getProjectDetails = { execute: vi.fn(() => of(projectDetails())) };

    await TestBed.configureTestingModule({
      imports: [ProjectDetailPage],
      providers: [
        provideRouter([]),
        { provide: GetProjectDetailsUseCase, useValue: getProjectDetails },
        {
          provide: ActivatedRoute,
          useValue: { paramMap: of(convertToParamMap({ id: 'project-1' })) },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailPage);
  });

  it('loads the route project and renders its complete detail in response order', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(getProjectDetails.execute).toHaveBeenCalledOnce();
    expect(getProjectDetails.execute).toHaveBeenCalledWith('project-1');
    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Virtual Buddy');
    expect(fixture.nativeElement.querySelector('.status').textContent).toContain('En revision');
    expect(fixture.nativeElement.textContent).toContain('Project description');
    expect(fixture.nativeElement.textContent).toContain('Clean Architecture');
    const technologies = fixture.nativeElement.querySelectorAll('.technology-list li');
    expect(technologies[0].textContent).toContain('Angular');
    expect(technologies[1].textContent).toContain('.NET');
    const members = fixture.nativeElement.querySelectorAll('.member-list li');
    expect(members[0].textContent).toContain('Maria Gonzalez');
    expect(members[1].textContent).toContain('Carlos Ruiz');
    expect(fixture.nativeElement.textContent).not.toContain('Consulta con IA');
  });

  it('announces loading while the detail request is pending', () => {
    getProjectDetails.execute.mockReturnValue(new Subject<ProjectDetails>());

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain(
      'Cargando proyecto...',
    );
  });

  it('renders explicit empty collections and omits empty architecture', async () => {
    getProjectDetails.execute.mockReturnValue(
      of({ ...projectDetails(), technologies: [], members: [], architectureInfo: '  ' }),
    );

    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('No hay tecnologias registradas.');
    expect(fixture.nativeElement.textContent).toContain('No hay miembros registrados.');
    expect(fixture.nativeElement.querySelector('.architecture')).toBeNull();
  });

  it('renders a non-retryable not found state', async () => {
    getProjectDetails.execute.mockReturnValue(throwError(() => new ProjectNotFoundError()));

    fixture.detectChanges();
    await fixture.whenStable();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert.textContent).toContain('Proyecto no encontrado.');
    expect(alert.querySelector('a').getAttribute('href')).toBe('/');
    expect(alert.querySelector('button')).toBeNull();
  });

  it('retries a recoverable error without reloading', async () => {
    getProjectDetails.execute
      .mockReturnValueOnce(throwError(() => new Error('Unavailable')))
      .mockReturnValueOnce(of(projectDetails()));

    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.nativeElement.textContent).toContain('No fue posible cargar el proyecto.');

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(getProjectDetails.execute).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Virtual Buddy');
  });

  function projectDetails(): ProjectDetails {
    return {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Project description',
      developmentStartedAt: '2026-03-07T10:00:00',
      status: ProjectStatus.Review,
      architectureInfo: 'Clean Architecture',
      technologies: [
        { id: 'technology-1', name: 'Angular' },
        { id: 'technology-2', name: '.NET' },
      ],
      members: [
        { userId: 'user-1', fullName: 'Maria Gonzalez', role: 'Project Lead' },
        { userId: 'user-2', fullName: 'Carlos Ruiz', role: 'Developer' },
      ],
    };
  }
});
