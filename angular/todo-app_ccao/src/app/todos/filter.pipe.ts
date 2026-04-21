import { Pipe, PipeTransform } from '@angular/core';

import * as actions from '../filter/filter.actions';
import { Todo } from './models/todo.model';

@Pipe({
  name: 'filterPipe'
})
export class FilterPipe implements PipeTransform {

public transform(todos: Todo[], filter: actions.filterState): Todo[] {
    switch (filter) {
      case actions.filterTypes.COMPLETED:
        return todos.filter(todo => todo.completed);
      case actions.filterTypes.PENDING:
        return todos.filter(todo => !todo.completed);
      default:
        return todos;
    }
  }

}
