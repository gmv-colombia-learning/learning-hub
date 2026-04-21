import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { TodoFooterComponent } from './todo-footer/todo-footer.component';
import { TodoAddComponent } from './todo-add/todo-add.component';
import { TodoItemComponent } from './todo-item/todo-item.component';
import { TodoListComponent } from './todo-list/todo-list.component';
import { TodoPageComponent } from './todo-page/todo-page.component';
import { FilterPipe } from './filter.pipe';



@NgModule({
  declarations: [
    TodoFooterComponent,
    TodoAddComponent,
    TodoItemComponent,
    TodoListComponent,
    TodoPageComponent,
    FilterPipe
  ],
  imports: [
    ReactiveFormsModule,
    CommonModule
  ],
  exports:[
    TodoPageComponent
  ]
})
export class TodoModule { }
