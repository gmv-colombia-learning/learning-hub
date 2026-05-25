import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";

import { Store } from "@ngrx/store";
import { AppState } from "../../../app.reducer";
import * as ui from "../../../shared/ui.actions";

import Swal from "sweetalert2";
import { AuthService } from "../../../shared/services/auth.service";
import { User as UserAuth } from "firebase/auth";

import { Subscription } from "rxjs";
import { CommonModule } from "@angular/common";

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  selector: "app-login",
  templateUrl: "./login.component.html",
  styles: [],
})
export class LoginComponent implements OnInit, OnDestroy {
  public loginForm!: FormGroup;
  public loading: boolean = false;
  private uiSubscription!: Subscription;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private store: Store<AppState>,
    private router: Router,
  ) {}

  ngOnInit() {
    this.loginForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
      password: ["", Validators.required],
    });

    this.uiSubscription = this.store.select(state => state.ui).subscribe((ui) => {
      this.loading = ui.isLoading;
    });
  }

  ngOnDestroy() {
    this.uiSubscription?.unsubscribe();
  }

  public login(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.store.dispatch(ui.isLoading());
    const { email, password } = this.loginForm.value;

    this.authService.login(email, password).subscribe({
      next: (user: UserAuth) => {
        this.store.dispatch(ui.stopLoading());
        this.router.navigate(["/"]);
      },
      error: (err) => {
        this.store.dispatch(ui.stopLoading());
        Swal.fire({
          icon: "error",
          title: "Oops...",
          text: err.message,
        });
      },
    });
  }
}
