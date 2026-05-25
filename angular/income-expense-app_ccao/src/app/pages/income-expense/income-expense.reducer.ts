import { createReducer, on } from "@ngrx/store";

import { setItems, unSetItems } from "./income-expense.actions";
import { IncomeExpense } from "../../shared/models/income-expense.model";
import { AppState } from "../../app.reducer";

export interface State {
  items: IncomeExpense[];
}

export interface AppStateWithIncome extends AppState {
  incomeExpense: State;
}

export const initialState: State = {
  items: [],
};

const _incomeExpenseReducer = createReducer(
  initialState,

  on(setItems, (state, { items }) => ({ ...state, items: [...items] })),
  on(unSetItems, (state) => ({ ...state, items: [] })),
);

export function incomeExpenseReducer(state: State | undefined, action: any) {
  return _incomeExpenseReducer(state, action);
}
