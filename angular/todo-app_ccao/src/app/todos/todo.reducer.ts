import { Action, createReducer, on } from '@ngrx/store';

import {
  addTodo,
  editTodo,
  deleteTodo,
  toggle,
  toggleAll,
  clearCompleted,
} from './todo.actions';
import { createTodo, Todo } from '../shared/models/todo.model';

export const initialState: Todo[] = [
  createTodo('Salvar el mundo'),
  createTodo('Vencer a Thanos'),
  createTodo('Comprar traje de Ironman'),
];

const _todoReducer = createReducer(
  initialState,
  on(addTodo, (state, { text }) => [...state, createTodo(text)]),
  on(toggle, (state, { id }) => {
    return state.map((todo: Todo) => {
      if (todo.id === id) {
        return { ...todo, completed: !todo.completed };
      }
      return todo;
    });
  }),
  on(editTodo, (state, { id, text }) => {
    return state.map((todo: Todo) => {
      if (todo.id === id) {
        return { ...todo, text };
      }
      return todo;
    });
  }),
  on(deleteTodo, (state, { id }) => {
    return state.filter((todo: Todo) => todo.id !== id);
  }),
  on(toggleAll, (state, { complete }) => {
    return state.map((todo: Todo) => {
      return { ...todo, completed: complete };
    });
  }),
  on(clearCompleted, (state) => {
    return state.filter((todo: Todo) => !todo.completed);
  }),
);

export function todoReducer(state: Todo[] | undefined, action: Action) {
  return _todoReducer(state, action);
}
