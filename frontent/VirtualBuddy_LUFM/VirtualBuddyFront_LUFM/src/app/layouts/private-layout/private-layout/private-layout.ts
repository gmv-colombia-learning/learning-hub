import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { SessionService } from '../../../features/user/infrastructure/session.service';

@Component({
  selector: 'app-private-layout',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './private-layout.html',
  styleUrl: './private-layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrivateLayout {
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);

  protected readonly user = this.session.user;

  protected logout(): void {
    this.session.clear();
    void this.router.navigate(['/login']);
  }
}
