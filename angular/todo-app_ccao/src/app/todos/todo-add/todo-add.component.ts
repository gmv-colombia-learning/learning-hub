import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { AppState } from 'src/app/app.reducer';
import * as actons from '../todo.actions';

@Component({
  selector: 'app-todo-add',
  templateUrl: './todo-add.component.html',
  styleUrls: ['./todo-add.component.css'],
})
export class TodoAddComponent {
  public taskInput: FormControl;

  constructor(private store: Store<AppState>) {
    this.taskInput = new FormControl('', Validators.required);
  }

  public addTask(): void {
    if (this.taskInput.valid) {
      this.store.dispatch(actons.addTodo({ text: this.taskInput.value }));
      this.taskInput.reset();
    }
  }
}
