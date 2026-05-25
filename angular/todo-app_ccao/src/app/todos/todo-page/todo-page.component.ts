import { Component } from '@angular/core';

import { Store } from '@ngrx/store';
import * as actions from '../todo.actions';

import { AppState } from 'src/app/app.reducer';

@Component({
  selector: 'app-todo-page',
  templateUrl: './todo-page.component.html',
  styleUrls: ['./todo-page.component.css'],
})
export class TodoPageComponent {
  public complete: boolean = false;

  constructor(private store: Store<AppState>) {}

  public toggleAll() {
    this.complete = !this.complete;
    this.store.dispatch(actions.toggleAll({ complete: this.complete }));
  }
}
