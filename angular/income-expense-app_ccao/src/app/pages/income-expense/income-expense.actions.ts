import { createAction, props } from "@ngrx/store";
import { IncomeExpense } from "../../shared/models/income-expense.model";

export const unSetItems = createAction("[IncomeExpense] Unset Items");

export const setItems = createAction(
  "[IncomeExpense] Set Items",
  props<{ items: IncomeExpense[] }>(),
);
