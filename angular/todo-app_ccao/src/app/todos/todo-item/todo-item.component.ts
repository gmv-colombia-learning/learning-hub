import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';

import { Store } from '@ngrx/store';
import * as actions from '../todo.actions';

import { AppState } from 'src/app/app.reducer';
import { Todo } from 'src/app/shared/models/todo.model';

@Component({
  selector: 'app-todo-item',
  templateUrl: './todo-item.component.html',
  styleUrls: ['./todo-item.component.css'],
})
export class TodoItemComponent implements OnInit {
  @Input() todoItem!: Todo;
  @ViewChild('editInput') editInput!: ElementRef;

  public chkComplete!: FormControl;
  public itemInput!: FormControl;
  public isEditing: boolean = false;

  constructor(private store: Store<AppState>) {}

  ngOnInit(): void {
    this.chkComplete = new FormControl(this.todoItem.completed);
    this.itemInput = new FormControl(this.todoItem.text, Validators.required);

    this.chkComplete.valueChanges.subscribe((value) => {
      this.store.dispatch(actions.toggle({ id: this.todoItem.id }));
    });
  }

  public editItem(): void {
    this.isEditing = true;
    this.itemInput.setValue(this.todoItem.text);

    setTimeout(() => {
      this.editInput.nativeElement.select();
    }, 1);
  }

  public endEditing(): void {
    this.isEditing = false;

    if (this.itemInput.invalid || this.itemInput.value === this.todoItem.text) {
      return;
    }

    this.store.dispatch(
      actions.editTodo({ id: this.todoItem.id, text: this.itemInput.value }),
    );
  }

  public deleteTodo(): void {
    this.store.dispatch(actions.deleteTodo({ id: this.todoItem.id }));
  }
}
