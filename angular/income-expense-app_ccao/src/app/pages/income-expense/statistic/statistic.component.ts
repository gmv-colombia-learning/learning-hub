import {
  Component,
  OnInit,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import Chart from 'chart.js/auto';

import { AppStateWithIncome } from '../income-expense.reducer';
import { MovementType } from '../../../shared/enums/movement-type';
import { IncomeExpense } from '../../../shared/models/income-expense.model';

@Component({
  standalone: true,
  selector: 'app-statistic',
  imports: [CommonModule],
  templateUrl: './statistic.component.html',
})
export class StatisticComponent implements OnInit {
  @ViewChild('chartCanvas')
  set chartCanvasRef(ref: ElementRef<HTMLCanvasElement> | undefined) {
    if (!ref) return;

    this.renderChart(ref.nativeElement);
  }

  public incomes: number = 0;
  public expenses: number = 0;

  public totalExpense: number = 0;
  public totalIncomes: number = 0;

  private chart!: Chart;

  constructor(private store: Store<AppStateWithIncome>) {}

  ngOnInit() {
    this.store
      .select(state => state.incomeExpense)
      .subscribe(({ items }) => this.generateStatistics(items));
  }

  private generateStatistics(items: IncomeExpense[]) {
    this.totalExpense = 0;
    this.totalIncomes = 0;
    this.incomes = 0;
    this.expenses = 0;

    for (const item of items) {
      if (item.type === MovementType.INCOME) {
        this.totalIncomes += item.amount;
        this.incomes++;
      } else {
        this.totalExpense += item.amount;
        this.expenses++;
      }
    }
  }

  private renderChart(canvas: HTMLCanvasElement) {
    const data = {
      labels: ['Ingresos', 'Egresos'],
      datasets: [
        {
          data: [this.totalIncomes, this.totalExpense],
          backgroundColor: ['#4caf50', '#f44336'],
          hoverOffset: 10,
        },
      ],
    };

    const options = {
      responsive: true,
      maintainAspectRatio: false,
    };

    if (this.chart) {
      this.chart.destroy();
    }

    this.chart = new Chart(canvas, {
      type: 'doughnut',
      data,
      options,
    });
  }
}
