import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { Country } from '../../../../interface/country.interface';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'country-information',
  imports: [DecimalPipe],
  templateUrl: './country-information.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CountryInformationComponent {
  country = input.required<Country>();

  currentYear = computed(() => new Date().getFullYear());
}
