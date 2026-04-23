import { Pipe, PipeTransform } from '@angular/core';

import { Todo } from '../models/todo.model';
import { FilterTypes } from '../enums/filter-type';
import { FilterState } from '../types/filter-state';


@Pipe({
  name: 'filterPipe'
})
export class FilterPipe implements PipeTransform {

public transform(todos: Todo[], filter: FilterState): Todo[] {
    switch (filter) {
      case FilterTypes.COMPLETED:
        return todos.filter(todo => todo.completed);
      case FilterTypes.PENDING:
        return todos.filter(todo => !todo.completed);
      default:
        return todos;
    }
  }

}
