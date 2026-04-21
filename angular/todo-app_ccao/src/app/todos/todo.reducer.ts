import { Action, createReducer, on } from "@ngrx/store";
import { Todo } from "./models/todo.model";
import { addTodo, editTodo, deleteTodo, toggle, toggleAll, clearCompleted } from "./todo.actions";


export const initialState: Todo[] = [new Todo('Salvar el mundo'), new Todo('Vencer a Thanos'), new Todo('Comprar traje de Ironman')];

const _todoReducer = createReducer(
  initialState,
  on(addTodo, (state, { text }) => [...state, new Todo(text)]),
  on(toggle, (state, { id }) => {
    return state.map(todo => {
      if (todo.id === id) {
        return { ...todo, completed: !todo.completed };
      }
      return todo;
    });
  }),
  on(editTodo, (state, { id, text }) => {
    return state.map(todo => {
      if (todo.id === id) {
        return { ...todo, text };
      }
      return todo;
    });
  }),
  on(deleteTodo, (state, { id }) => {
    return state.filter(todo => todo.id !== id);
  }),
  on(toggleAll, (state, { complete }) => {
    return state.map(todo => {
      return { ...todo, completed: complete };
    });
  }),
  on(clearCompleted, (state) => {
    return state.filter(todo => !todo.completed);
  })
);

export function todoReducer(state: Todo[] | undefined, action: Action) {
  return _todoReducer(state, action);
}
