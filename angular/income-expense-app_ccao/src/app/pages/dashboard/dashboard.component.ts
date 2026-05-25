import { FooterComponent } from './../../shared/components/footer/footer.component';
import { Component, OnInit, OnDestroy } from "@angular/core";

import { Store } from "@ngrx/store";
import * as incomeExpenseActions from "../income-expense/income-expense.actions";

import { Subscription } from "rxjs";
import { filter } from "rxjs/operators";

import { CommonModule } from "@angular/common";
import { AppState } from "../../app.reducer";
import { IncomeExpenseService } from "../../shared/services/income-expense.service";
import { NavbarComponent } from "../../shared/components/navbar/navbar.component";
import { SidebarComponent } from "../../shared/components/sidebar/sidebar.component";
import { RouterModule } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule, NavbarComponent, SidebarComponent, FooterComponent, RouterModule],
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styles: [],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private userSubs!: Subscription;
  private incomeSubs!: Subscription;

  constructor(
    private store: Store<AppState>,
    private incomeExpenseService: IncomeExpenseService,
  ) {}

  ngOnInit() {
    this.userSubs = this.store
      .select(state => state.user)
      .pipe(filter((auth) => auth.user != null))
      .subscribe(({ user }) => {
        if (user) {
          this.incomeSubs = this.incomeExpenseService
            .initIncomeExpenseListener(user.uid)
            .subscribe((incomeExpenseFB) => {
              this.store.dispatch(
                incomeExpenseActions.setItems({ items: incomeExpenseFB }),
              );
            });
        }
      });
  }

  ngOnDestroy() {
    this.incomeSubs?.unsubscribe();
    this.userSubs?.unsubscribe();
  }
}
