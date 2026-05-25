import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppState } from 'src/app/app.reducer';
import { FilterState } from 'src/app/shared/types/filter-state';
import { Todo } from 'src/app/shared/models/todo.model';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.css'],
})
export class TodoListComponent implements OnInit {
  public todosList: Todo[] = [];
  public actualFilter!: FilterState;

  constructor(private store: Store<AppState>) {}

  ngOnInit(): void {
    this.store.subscribe(({ todos, filter }) => {
      this.todosList = todos;
      this.actualFilter = filter;
    });
  }
}
