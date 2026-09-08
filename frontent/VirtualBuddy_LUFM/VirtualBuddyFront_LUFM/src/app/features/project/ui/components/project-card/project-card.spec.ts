import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ProjectStatus } from '../../../domain/project-summary';
import { ProjectCard } from './project-card';

describe('ProjectCard', () => {
  let fixture: ComponentFixture<ProjectCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectCard],
      providers: [provideRouter([])],
    }).compileComponents();
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

  it('renders the approved project summary as a single detail link', () => {
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
    const links = fixture.nativeElement.querySelectorAll('a');
    expect(links).toHaveLength(1);
    expect(links[0].getAttribute('href')).toBe('/projects/project-1');
    expect(fixture.nativeElement.querySelector('button')).toBeNull();
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
