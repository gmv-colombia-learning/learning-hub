import { User } from "./req-response";

export interface State {
  users: User[];
  loading: boolean;
}
