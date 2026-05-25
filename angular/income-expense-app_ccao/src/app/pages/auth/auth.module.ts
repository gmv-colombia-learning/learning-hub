import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";

import { LoginComponent } from "./login/login.component";
import { RegisterComponent } from "./register/register.component";

@NgModule({
  declarations: [],
  imports: [LoginComponent, RegisterComponent, CommonModule, ReactiveFormsModule, RouterModule],
})
export class AuthModule {}
