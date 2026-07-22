import { Component, input } from '@angular/core';
import { Object as Country } from '../../interface/rest-countries.interfaces';

@Component({
  selector: 'country-list',
  imports: [],
  templateUrl: './country-list.component.html',
})
export class CountryListComponent {
  countries = input.required<Country[]>();
}
