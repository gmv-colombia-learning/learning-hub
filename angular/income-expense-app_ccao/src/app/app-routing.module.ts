import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";

import { LoginComponent } from "./pages/auth/login/login.component";
import { RegisterComponent } from "./pages/auth/register/register.component";
import { authGuard } from "./shared/guards/auth.guard";


export const routes: Routes = [
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  {
    path: "",
    canMatch: [authGuard],
    loadChildren() {
      return import("./pages/income-expense/income-expense.module").then(
        (m) => m.IncomeExpenseModule,
      );
    },
  },
  { path: "**", redirectTo: "" },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
