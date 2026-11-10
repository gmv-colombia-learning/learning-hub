import { environment } from '../../../environments/environment.local';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import type { RESTCountry } from '../interface/rest-countries.interfaces';
import { catchError, delay, map, Observable, of, throwError } from 'rxjs';
import type { Country } from '../interface/country.interface';
import { CountryMapper } from '../mappers/country.mapper';
import type { Region } from '../interface/region.type';


const API_URL = 'https://api.restcountries.com/countries/v5'

@Injectable({
  providedIn: 'root',
})
export class CountryService {

  private http = inject(HttpClient);

  private readonly headers = new HttpHeaders({
    Authorization: environment.restCountriesAuthorization
  });

  searchByCapital(query: string): Observable<Country[]> {
    const normalize = (value: string) =>
      value.trim().toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');

    const prefix = normalize(query);

    return this.http.get<RESTCountry>(`${API_URL}/capitals`, {
      headers: this.headers,
      params: { q: query.trim(), limit: 100 },
    }).pipe(
      map((response) => {
        const countries = response.data.objects
          .map(country => ({
            ...country,
            capitals: country.capitals.filter(capital =>
              normalize(capital.name).startsWith(prefix)
            ),
          }))
          .filter(country => country.capitals.length > 0);

        return CountryMapper.mapRestCountryArrayToCountryArray(countries);
      }),
      catchError(() => {
        return throwError(
          () => new Error(`No se pudo obtener países con ese query ${query}`)
        )
      })
    );
  }

  searchByCountry(query: string): Observable<Country[]> {
    const normalize = (value: string) =>
      value.trim().toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '');

    const prefix = normalize(query);

    return this.http.get<RESTCountry>(`${API_URL}/name`, {
      headers: this.headers,
      params: { q: query.trim(), limit: 100 },
    }).pipe(
      map((response) => {
        const countries = response.data.objects.filter(country =>
          normalize(country.names.common).startsWith(prefix)
        );

        return CountryMapper.mapRestCountryArrayToCountryArray(countries);
      }),
      catchError(() => {
        return throwError(
          () => new Error(`No se pudo obtener países con ese query ${query}`)
        )
      })
    );
  }

  searchByRegion(region: Region): Observable<Country[]> {
    return this.http.get<RESTCountry>(API_URL, {
      headers: this.headers,
      params: { region, limit: 100 },
    }).pipe(
      map((response) =>
        CountryMapper.mapRestCountryArrayToCountryArray(response.data.objects)
      ),
      catchError(() =>
        throwError(
          () => new Error(`No se pudieron obtener los países de la región "${region}".`)
        )
      )
    );
  }

  searchCountryByAlphaCode(code: string): Observable<Country | undefined> {
    const normalizedCode = encodeURIComponent(code.trim().toUpperCase());
    const url = `${API_URL}/codes.alpha_2/${normalizedCode}`;

    return this.http.get<RESTCountry>(url, {
      headers: this.headers,
    }).pipe(
      map((response) =>
        CountryMapper.mapRestCountryArrayToCountryArray(
          response.data.objects
        )
      ),
      map((countries) => countries.at(0)),
      catchError((error) => {
        console.error('Error fetching country by code:', error);

        return throwError(
          () => new Error(`No se pudo obtener el país con el código "${code}".`)
        );
      })
    );
  }
}
