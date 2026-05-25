import { Action, createReducer, on } from '@ngrx/store';

import { setFilter } from './filter.actions';
import { FilterState } from '../shared/types/filter-state';
import { FilterTypes } from '../shared/enums/filter-type';

export const initialState: FilterState = FilterTypes.ALL as FilterState;

const _filterReducer = createReducer(
  initialState,
  on(setFilter, (state, { filter }) => filter)
);

export function filterReducer(state: FilterState | undefined, action: Action) {
  return _filterReducer(state, action);
}
