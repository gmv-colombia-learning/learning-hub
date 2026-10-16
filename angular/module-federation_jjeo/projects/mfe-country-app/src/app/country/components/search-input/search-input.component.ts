import { Component, input, output } from '@angular/core';

@Component({
  selector: 'country-search-input',
  templateUrl: './search-input.component.html',
})
export class SearchInputComponent {
  placeholder = input('Buscar');
  value = output<string>();
}
