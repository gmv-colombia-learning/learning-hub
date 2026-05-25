import { Component, OnDestroy, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from "@angular/forms";
import { Router } from "@angular/router";

import { Store } from "@ngrx/store";
import { AppState } from "../../../app.reducer";
import * as ui from "../../../shared/ui.actions";

import Swal from "sweetalert2";
import { AuthService } from "../../../shared/services/auth.service";
import { User } from "../../../shared/models/user.model";

import { Subscription } from "rxjs";
import { CommonModule } from "@angular/common";

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: "app-register",
  templateUrl: "./register.component.html",
  styles: [],
})
export class RegisterComponent implements OnInit, OnDestroy {
  public registroForm!: FormGroup;
  public loading: boolean = false;
  private uiSubscription!: Subscription;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private store: Store<AppState>,
    private router: Router,
  ) {}

  ngOnInit() {
    this.registroForm = this.fb.group({
      name: ["", Validators.required],
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

  public createUser(): void {
    if (this.registroForm.invalid) {
      return;
    }

    this.store.dispatch(ui.isLoading());
    const { name, email, password } = this.registroForm.value;

    this.authService
      .createUser(name, email, password)
      .then((user: User) => {
        this.store.dispatch(ui.stopLoading());
        this.router.navigate(["/"]);
      })
      .catch((err) => {
        this.store.dispatch(ui.stopLoading());
        Swal.fire({
          icon: "error",
          title: "Oops...",
          text: err.message,
        });
      });
  }
}
