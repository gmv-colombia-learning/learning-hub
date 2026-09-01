import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProjectStatus } from '../../../domain/project-summary';
import { ProjectCard } from './project-card';

describe('ProjectCard', () => {
  let fixture: ComponentFixture<ProjectCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ProjectCard] }).compileComponents();
    fixture = TestBed.createComponent(ProjectCard);
  });

  it.each([
    [ProjectStatus.Unknown, 'Desconocido'],
    [ProjectStatus.Active, 'Activo'],
    [ProjectStatus.Inactive, 'Inactivo'],
    [ProjectStatus.Review, 'En revision'],
    [ProjectStatus.Completed, 'Completado'],
  ])('renders status %i as %s', (status, label) => {
    fixture.componentRef.setInput('project', {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Mentoria virtual',
      status,
      imageUrl: 'project.png',
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.status').textContent.trim()).toBe(label);
  });

  it('renders the approved project summary without an interactive action', () => {
    fixture.componentRef.setInput('project', {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Mentoria virtual',
      status: ProjectStatus.Active,
      imageUrl: 'project.png',
    });
    fixture.detectChanges();

    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(image.alt).toBe('Virtual Buddy');
    expect(fixture.nativeElement.textContent).toContain('Mentoria virtual');
    expect(fixture.nativeElement.querySelector('a, button')).toBeNull();
  });

  it('uses the fallback image when the project has no image URL', () => {
    fixture.componentRef.setInput('project', {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Mentoria virtual',
      status: ProjectStatus.Active,
      imageUrl: '',
    });
    fixture.detectChanges();

    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    expect(image.getAttribute('src')).toBe('/sin-imagen.png');
    expect(image.classList).toContain('project-image--fallback');
  });

  it('uses the fallback image when the project image fails to load', () => {
    fixture.componentRef.setInput('project', {
      id: 'project-1',
      name: 'Virtual Buddy',
      description: 'Mentoria virtual',
      status: ProjectStatus.Active,
      imageUrl: '/missing-project.png',
    });
    fixture.detectChanges();

    const image = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    image.dispatchEvent(new Event('error'));

    expect(image.getAttribute('src')).toBe('/sin-imagen.png');
    expect(image.classList).toContain('project-image--fallback');
  });
});
