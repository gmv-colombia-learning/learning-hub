import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { SessionService } from '../../../features/user/infrastructure/session.service';
import { PrivateLayout } from './private-layout';

describe('PrivateLayout', () => {
  let component: PrivateLayout;
  let fixture: ComponentFixture<PrivateLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrivateLayout],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(PrivateLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('clears the session and navigates to login', () => {
    const session = TestBed.inject(SessionService);
    const router = TestBed.inject(Router);
    const clear = vi.spyOn(session, 'clear');
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();

    expect(clear).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
