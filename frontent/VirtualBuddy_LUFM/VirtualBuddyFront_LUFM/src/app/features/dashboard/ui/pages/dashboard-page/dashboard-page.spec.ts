import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { ListProjectsUseCase } from '../../../../project/application/list-projects.use-case';
import { ProjectStatus, ProjectSummary } from '../../../../project/domain/project-summary';
import { DashboardPage } from './dashboard-page';

describe('DashboardPage', () => {
  let component: DashboardPage;
  let fixture: ComponentFixture<DashboardPage>;
  let listProjects: { execute: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    listProjects = { execute: vi.fn(() => of(projects)) };
    await TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [{ provide: ListProjectsUseCase, useValue: listProjects }],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardPage);
    component = fixture.componentInstance;
  });

  it('loads and displays every project in response order', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component).toBeTruthy();
    expect(listProjects.execute).toHaveBeenCalledOnce();
    const cards = fixture.nativeElement.querySelectorAll('app-project-card');
    expect(cards).toHaveLength(2);
    expect(cards[0].textContent).toContain('Second project');
    expect(cards[1].textContent).toContain('First project');
    expect(fixture.nativeElement.textContent).not.toContain('Administracion');
  });

  it('announces loading while the request is pending', () => {
    listProjects.execute.mockReturnValue(new Subject<readonly ProjectSummary[]>());

    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain(
      'Cargando proyectos...',
    );
  });

  it('displays the approved empty state', async () => {
    listProjects.execute.mockReturnValue(of([]));

    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('No hay proyectos disponibles.');
    expect(fixture.nativeElement.querySelector('app-project-card')).toBeNull();
  });

  it('displays an error and retries without reloading the page', async () => {
    listProjects.execute
      .mockReturnValueOnce(throwError(() => new Error('Unavailable')))
      .mockReturnValueOnce(of(projects));

    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'No fue posible cargar los proyectos.',
    );

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(listProjects.execute).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelectorAll('app-project-card')).toHaveLength(2);
  });

  const projects: readonly ProjectSummary[] = [
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
});
