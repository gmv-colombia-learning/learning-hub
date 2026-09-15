import { environment } from '../../../environments/environment.local';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { RESTCountry } from '../interface/rest-countries.interfaces';
import { catchError, map, Observable, throwError } from 'rxjs';
import { Country } from '../interface/country.interface';
import { CountryMapper } from '../mappers/country.mapper';


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
      catchError((error) => {
        console.error('Error fetching countries by capital:', error);
        
        return throwError (
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
      catchError((error) => {
        console.error('Error fetching countries by name:', error);
        
        return throwError (
          () => new Error(`No se pudo obtener países con ese query ${query}`)
        )
      })
    );
  }
}