import { Action, createReducer, on } from '@ngrx/store';

import { setFilter, filterState, filterTypes } from './filter.actions';

export const initialState: filterState = filterTypes.ALL as filterState;

const _filterReducer = createReducer(
  initialState,
  on(setFilter, (state, { filter }) => filter)
);

export function filterReducer(state: filterState | undefined, action: Action) {
  return _filterReducer(state, action);
}
