
import { Component } from '@angular/core';
import { AuthService } from './shared/services/auth.service';
import { RouterOutlet } from '@angular/router';

@Component({
  standalone: true,
  imports: [RouterOutlet],
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';

  constructor(private authService: AuthService) {
    this.authService.initAuthListener();
  }

}
