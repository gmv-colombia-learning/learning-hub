import { Pipe, PipeTransform } from "@angular/core";

import { IncomeExpense } from "../models/income-expense.model";
import { MovementType } from "../enums/movement-type";

@Pipe({
  name: 'incomeOrder',
  standalone: true
})

export class IncomeOrderPipe implements PipeTransform {
  transform(items: IncomeExpense[]): IncomeExpense[] {
    return [...items].sort((a, b) => {
      return a.type === MovementType.INCOME ? -1 : 1;
    });
  }
}
