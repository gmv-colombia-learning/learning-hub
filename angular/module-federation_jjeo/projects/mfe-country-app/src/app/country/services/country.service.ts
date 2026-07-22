import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { RESTCountry } from '../interface/rest-countries.interfaces';


const API_URL = 'https://api.restcountries.com/countries/v5'


@Injectable({
  providedIn: 'root',
})
export class CountryService {

  private http = inject(HttpClient);

  private readonly headers = new HttpHeaders({
    Authorization: 'Bearer rc_live_5386f5bfa5514441865e4751f2bacee9'
  });


  searchByCapital(query: string) {
    query = query.toLowerCase();

    return this.http.get<RESTCountry>(
      `${API_URL}/capitals/${query}`,
      { headers: this.headers },
    );
  }
}