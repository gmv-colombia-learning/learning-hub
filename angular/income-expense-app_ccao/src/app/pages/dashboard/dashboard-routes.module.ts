import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";

import { dashboardRoutes } from "./dashboard.routes";
import { DashboardComponent } from "./dashboard.component";

const childRoutes: Routes = [
  {
    path: "",
    component: DashboardComponent,
    children: dashboardRoutes,
  },
];

@NgModule({
  declarations: [],
  imports: [RouterModule.forChild(childRoutes)],
  exports: [RouterModule],
})
export class DashboardRoutesModule {}
