import { Router, RouterLink } from "@angular/router";
import { Component, OnDestroy, OnInit } from "@angular/core";

import { Store } from "@ngrx/store";
import { Subscription } from "rxjs";

import { AuthService } from "./../../services/auth.service";
import { AppState } from "../../../app.reducer";
import { CommonModule } from "@angular/common";

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styles: [],
})
export class SidebarComponent implements OnInit, OnDestroy {
  public userName: string = "";
  private userSubs!: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    private store: Store<AppState>,
  ) {}

  ngOnInit() {
    this.userSubs = this.store
      .select(state => state.user)
      .subscribe(({ user }) => (this.userName = user?.name || ""));
  }

  ngOnDestroy(): void {
    this.userSubs?.unsubscribe();
  }

  public logout() {
    this.authService.logout().subscribe(() => {
      this.router.navigate(["/login"]);
    });
  }
}
