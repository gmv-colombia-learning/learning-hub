import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthenticationError } from '../../../application/authentication.error';
import { LoginUseCase } from '../../../application/login.use-case';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let login: { execute: ReturnType<typeof vi.fn> };
  let router: { navigateByUrl: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    login = { execute: vi.fn() };
    router = { navigateByUrl: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        { provide: LoginUseCase, useValue: login },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ returnUrl: '/projects' }) } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();
  });

  it('does not submit invalid values and displays field errors', () => {
    submitForm();
    fixture.detectChanges();

    expect(login.execute).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('El correo es obligatorio.');
    expect(fixture.nativeElement.textContent).toContain('La contrasena es obligatoria.');
  });

  it('submits credentials and restores the requested private URL', () => {
    login.execute.mockReturnValue(
      of({ token: 'jwt', email: 'user@example.com', fullName: 'User' }),
    );
    fillInput('#email', 'user@example.com');
    fillInput('#password', 'Password1');

    submitForm();

    expect(login.execute).toHaveBeenCalledWith({
      email: 'user@example.com',
      password: 'Password1',
    });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/projects');
  });

  it('uses one message for rejected credentials', () => {
    login.execute.mockReturnValue(throwError(() => new AuthenticationError('invalid-credentials')));
    fillInput('#email', 'user@example.com');
    fillInput('#password', 'wrong');

    submitForm();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain(
      'Correo o contrasena incorrectos.',
    );
  });

  function fillInput(selector: string, value: string): void {
    const input = fixture.nativeElement.querySelector(selector) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  function submitForm(): void {
    fixture.nativeElement
      .querySelector('form')
      .dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
  }
});
