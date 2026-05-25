import { Component, OnInit, OnDestroy } from "@angular/core";
import { FormGroup, FormBuilder, Validators, ReactiveFormsModule } from "@angular/forms";
import Swal from "sweetalert2";
import { Store } from "@ngrx/store";
import { AppState } from "../../app.reducer";
import * as ui from "../../shared/ui.actions";
import { Subscription } from "rxjs";
import { MovementType } from "../../shared/enums/movement-type";
import { IncomeExpenseService } from "../../shared/services/income-expense.service";
import { IncomeExpense } from "../../shared/models/income-expense.model";
import { CommonModule } from "@angular/common";


@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: "app-income-expense",
  templateUrl: "./income-expense.component.html",
  styles: [],
})
export class IncomeExpenseComponent implements OnInit, OnDestroy {
  public incomeForm!: FormGroup;
  public type: MovementType = MovementType.INCOME;
  public loading: boolean = false;
  public movementType = MovementType;
  private loadingSubs!: Subscription;

  constructor(
    private fb: FormBuilder,
    private incomeExpenseService: IncomeExpenseService,
    private store: Store<AppState>,
  ) {}

  ngOnInit() {
    this.loadingSubs = this.store
      .select(state => state.ui)
      .subscribe(({ isLoading }) => (this.loading = isLoading));

    this.incomeForm = this.fb.group({
      description: ["", Validators.required],
      amount: ["", Validators.required],
    });
  }

  ngOnDestroy() {
    this.loadingSubs?.unsubscribe();
  }

  public save(): void {
    if (this.incomeForm.invalid) {
      return;
    }

    this.store.dispatch(ui.isLoading());

    const { description, amount } = this.incomeForm.value;

    const incomeExpense: IncomeExpense = {
      description,
      amount,
      type: this.type,
    };

    this.incomeExpenseService
      .createIncomeExpense(incomeExpense)
      .then(() => {
        this.incomeForm.reset();
        this.store.dispatch(ui.stopLoading());
        Swal.fire("Registro creado", description, "success");
      })
      .catch((err: { message: string }) => {
        this.store.dispatch(ui.stopLoading());
        Swal.fire("Error", err.message, "error");
      });
  }
}
