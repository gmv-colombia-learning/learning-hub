export interface Todo {
  id: number;
  text: string;
  completed: boolean;
}

export const createTodo = (text: string): Todo => ({
  id: Math.random(),
  text,
  completed: false,
});
