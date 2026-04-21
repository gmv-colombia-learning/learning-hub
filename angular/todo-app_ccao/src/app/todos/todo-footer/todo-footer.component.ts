import { Component } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppState } from 'src/app/app.reducer';
import * as actions from '../../filter/filter.actions';
import * as actionsTodo from '../todo.actions';

@Component({
  selector: 'app-todo-footer',
  templateUrl: './todo-footer.component.html',
  styleUrls: ['./todo-footer.component.css'],
})
export class TodoFooterComponent {
  public actualFilter: actions.filterState = actions.filterTypes.ALL;
  public filters: actions.filterState[] = Object.values(actions.filterTypes);
  public pendings: number = 0;

  constructor(private store: Store<AppState>) {}
  ngOnInit(): void {
    this.store.subscribe(({ todos, filter }) => {
      this.actualFilter = filter;
      this.pendings = todos.filter((todo) => !todo.completed).length;
    });
  }

  public selectedFilter(filter: actions.filterState): void {
    this.store.dispatch(actions.setFilter({ filter }));
  }

  public clearTodo(): void {
    this.store.dispatch(actionsTodo.clearCompleted());
  }
}
