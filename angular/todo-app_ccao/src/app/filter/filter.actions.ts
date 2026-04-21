import { createAction, props } from '@ngrx/store';

export type filterState = 'ALL' | 'COMPLETED' | 'PENDING';

export enum filterTypes {
  ALL = 'ALL',
  COMPLETED = 'COMPLETED',
  PENDING = 'PENDING',
}

export const setFilter = createAction(
  '[Filter] Set Filter',
  props<{ filter: filterState }>()
);
