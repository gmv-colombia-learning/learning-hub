import type { Country } from '../interfaces/country.interface';
import type { genericResponse, RESTCountry } from '../interfaces/rest-countries.interface';

export class CountryMapper {
  static mapRestCountryToCountry(restCountry: RESTCountry): Country {
    return {
      capital: restCountry.capitals?.map(cap => cap.name).join(','),
      cca2: restCountry.codes.alpha_2,
      flagSvg: restCountry.flag.url_svg,
      name: restCountry.names.translations['spa']?.common ?? 'No Spanish Name',
      population: restCountry.population,

      region: restCountry.region,
      subRegion: restCountry.subregion,
    };
  }

  static mapRestCountryArrayToCountryArray(
    restCountries: genericResponse<RESTCountry>
  ): Country[] {
    return restCountries.data.objects.map(this.mapRestCountryToCountry);
  }
}
