import { Component, inject, Injector, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { CountryListComponent } from '../../components/country-list/country-list.component';
import type { Region } from '../../interface/region.type';
import { CountryService } from '../../services/country.service';

@Component({
  selector: 'app-by-region-page',
  imports: [CountryListComponent],
  templateUrl: './by-region-page.component.html',
})
export class ByRegionPageComponent {
  private readonly injector = inject(Injector);
  countryService = inject(CountryService);

  readonly regions: { value: Region; label: string }[] = [
    { value: 'Africa', label: 'África' },
    { value: 'Americas', label: 'América' },
    { value: 'Asia', label: 'Asia' },
    { value: 'Europe', label: 'Europa' },
    { value: 'Oceania', label: 'Oceanía' },
    { value: 'Antarctic', label: 'Antártida' },
  ];

  selectedRegion = signal<Region | null>(null);

  countryResource = rxResource({
    injector: this.injector,
    params: () => ({ region: this.selectedRegion() }),
    defaultValue: [],
    stream: ({ params }) => {
      if (!params.region) return of([]);

      return this.countryService.searchByRegion(params.region);
    },
  });
}
