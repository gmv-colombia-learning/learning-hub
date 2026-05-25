import { createAction, props } from '@ngrx/store';

import { FilterState } from '../shared/types/filter-state';

export const setFilter = createAction(
  '[Filter] Set Filter',
  props<{ filter: FilterState }>()
);
