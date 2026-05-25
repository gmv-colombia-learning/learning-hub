import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { StoreModule } from '@ngrx/store';
import { provideState } from '@ngrx/store';

import { DashboardRoutesModule } from '../dashboard/dashboard-routes.module';
import { incomeExpenseReducer } from './income-expense.reducer';

import { SharedModule } from '../../shared/shared.module';

import { DashboardComponent } from '../dashboard/dashboard.component';
import { DetailComponent } from './detail/detail.component';
import { IncomeExpenseComponent } from './income-expense.component';
import { StatisticComponent } from './statistic/statistic.component';
import { IncomeOrderPipe } from '../../shared/pipes/income-order.pipe';

@NgModule({
  declarations: [],
  providers: [provideState('incomeExpense', incomeExpenseReducer)],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DashboardRoutesModule,
    SharedModule,
    DashboardComponent,
    DetailComponent,
    IncomeExpenseComponent,
    StatisticComponent,
    IncomeOrderPipe,
  ],
})
export class IncomeExpenseModule {}
