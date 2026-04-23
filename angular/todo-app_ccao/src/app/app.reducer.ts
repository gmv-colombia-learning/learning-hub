import { ActionReducerMap } from "@ngrx/store";

import { Todo } from "./shared/models/todo.model";
import { todoReducer } from "./todos/todo.reducer";
import { filterReducer } from "./filter/filter.reducer";
import { FilterState } from "./shared/types/filter-state";

export interface AppState {
  todos: Todo[],
  filter: FilterState
}

export const appReducers: ActionReducerMap<AppState> = {
  todos: todoReducer,
  filter: filterReducer
}
