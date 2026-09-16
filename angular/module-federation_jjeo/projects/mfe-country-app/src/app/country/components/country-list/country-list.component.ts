import { Component, input } from '@angular/core';
import type { Country } from '../../interface/country.interface';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'country-list',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './country-list.component.html',
})

export class CountryListComponent {
  countries = input<Country[]>([]);
  errorMessage = input<string>();
  isLoading = input<boolean>(false);
  isEmpty = input<boolean>(false);
}
