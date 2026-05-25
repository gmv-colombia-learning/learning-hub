import { Component, OnDestroy, OnInit } from "@angular/core";
import { Store } from "@ngrx/store";
import { Subscription } from "rxjs";
import Swal from "sweetalert2";

import { AppStateWithIncome } from "../income-expense.reducer";
import { IncomeExpense } from "../../../shared/models/income-expense.model";
import { MovementType } from "../../../shared/enums/movement-type";
import { IncomeExpenseService } from "../../../shared/services/income-expense.service";
import { CommonModule } from "@angular/common";
import { IncomeOrderPipe } from "../../../shared/pipes/income-order.pipe";

@Component({
  standalone: true,
  imports: [CommonModule, IncomeOrderPipe],
  selector: "app-detail",
  templateUrl: "./detail.component.html",
  styles: [],
})

export class DetailComponent implements OnInit, OnDestroy {
  public incomeexpense: IncomeExpense[] = [];
  private incomeSubs!: Subscription;
  public movementType = MovementType;

  constructor(
    private store: Store<AppStateWithIncome>,
    private incomeExpenseService: IncomeExpenseService,
  ) {}

  ngOnInit() {
    this.incomeSubs = this.store
      .select(state => state.incomeExpense)
      .subscribe(({ items }) => (this.incomeexpense = items));
  }
  ngOnDestroy() {
    this.incomeSubs?.unsubscribe();
  }

  public deleteIncomeExpense(uid?: string): void {
    if (uid) {
      this.incomeExpenseService
        .deleteIncomeExpense(uid)
        .then(() => Swal.fire("Borrado", "Item borrado", "success"))
        .catch((err) => Swal.fire("Error", err.message, "error"));
    }
  }
}
