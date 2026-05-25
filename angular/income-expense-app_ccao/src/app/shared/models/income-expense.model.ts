import { MovementType } from "../enums/movement-type";

export interface IncomeExpense {
  description: string;
  amount: number;
  type: MovementType;
  uid?: string;
}
