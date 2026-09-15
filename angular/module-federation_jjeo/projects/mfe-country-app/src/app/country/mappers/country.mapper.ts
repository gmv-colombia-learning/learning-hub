import type { Country } from "../interface/country.interface";
import type { Object as RESTCountryItem } from "../interface/rest-countries.interfaces";


export class CountryMapper {

    static mapRestCountryToCountry( restCountry: RESTCountryItem ): Country {
        return {
            capitals: restCountry.capitals.map(capital => capital.name).join(', '),
            cca2: restCountry.codes.alpha_2,
            flag: restCountry.flag.emoji,
            flagSvg: restCountry.flag.url_svg,
            name: restCountry.names.common,
            population: restCountry.population,
        };
    }

    static mapRestCountryArrayToCountryArray( 
        restCountries: RESTCountryItem[]
    ): Country[] {
        return restCountries.map(this.mapRestCountryToCountry);
    } 
}
