import { Routes } from "@angular/router";
import { IncomeExpenseComponent } from "../income-expense/income-expense.component";
import { StatisticComponent } from "../income-expense/statistic/statistic.component";
import { DetailComponent } from "../income-expense/detail/detail.component";

export const dashboardRoutes: Routes = [
  { path: "", component: StatisticComponent },
  { path: "income-expense", component: IncomeExpenseComponent },
  { path: "detail", component: DetailComponent },
];
