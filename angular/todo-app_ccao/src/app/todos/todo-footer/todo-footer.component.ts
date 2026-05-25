import { Component } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppState } from 'src/app/app.reducer';
import * as actions from '../../filter/filter.actions';
import * as actionsTodo from '../todo.actions';
import { FilterState } from 'src/app/shared/types/filter-state';
import { FilterTypes } from 'src/app/shared/enums/filter-type';

@Component({
  selector: 'app-todo-footer',
  templateUrl: './todo-footer.component.html',
  styleUrls: ['./todo-footer.component.css'],
})
export class TodoFooterComponent {
  public actualFilter: FilterState = FilterTypes.ALL;
  public filters: FilterState[] = Object.values(FilterTypes);
  public pendings: number = 0;

  constructor(private store: Store<AppState>) {}
  ngOnInit(): void {
    this.store.subscribe(({ todos, filter }) => {
      this.actualFilter = filter;
      this.pendings = todos.filter((todo) => !todo.completed).length;
    });
  }

  public selectedFilter(filter: FilterState): void {
    this.store.dispatch(actions.setFilter({ filter }));
  }

  public clearTodo(): void {
    this.store.dispatch(actionsTodo.clearCompleted());
  }
}
