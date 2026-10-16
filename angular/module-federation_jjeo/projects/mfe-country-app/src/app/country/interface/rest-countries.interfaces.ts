export interface RESTCountry {
    data: Data;
}

export interface Data {
    objects: Object[];
    meta:    Meta;
}

export interface Meta {
    total:      number;
    count:      number;
    limit:      number;
    offset:     number;
    more:       boolean;
    request_id: string;
    duration:   number;
}

export interface Object {
    names:           Names;
    codes:           Codes;
    capitals:        Capital[];
    flag:            Flag;
    region:          string;
    subregion:       string;
    area:            Area;
    assets:          any[];
    borders:         any[];
    calling_codes:   string[];
    cars:            Cars;
    classification:  Classification;
    continents:      string[];
    coordinates:     Coordinates;
    currencies:      Currency[];
    date:            DateClass;
    demonyms:        Demonyms;
    economy:         Economy;
    government_type: string;
    landlocked:      boolean;
    languages:       Language[];
    leaders:         Leader[];
    links:           Links;
    memberships:     { [key: string]: boolean };
    number_format:   NumberFormat;
    parent:          Parent;
    population:      number;
    postal_code:     PostalCode;
    timezones:       string[];
    tlds:            string[];
    uuid:            string;
    _match:          Match[];
    _meta:           MetaClass;
}

export interface Match {
    path:  string;
    value: string;
}

export interface MetaClass {
    lastUpdatedTimestamp: number;
}

export interface Area {
    kilometers: number;
    miles:      number;
}

export interface Capital {
    attributes:  Attributes;
    coordinates: Coordinates;
    name:        string;
}

export interface Attributes {
    administrative: boolean;
    constitutional: boolean;
    executive:      boolean;
    judicial:       boolean;
    legislative:    boolean;
    primary:        boolean;
}

export interface Coordinates {
    lat: number;
    lng: number;
}

export interface Cars {
    driving_side: string;
    signs:        string[];
}

export interface Classification {
    dependency:      boolean;
    dependency_type: string;
    disputed:        boolean;
    iso_status:      string;
    sovereign:       boolean;
    un_member:       boolean;
    un_observer:     boolean;
}

export interface Codes {
    alpha_2: string;
    alpha_3: string;
    ccn3:    string;
    cioc:    string;
    fifa:    string;
    fips:    string;
    gec:     string;
}

export interface Currency {
    code:   string;
    name:   string;
    symbol: string;
}

export interface DateClass {
    academic_year_start: AcademicYearStart;
    fiscal_year_start:   FiscalYearStart;
    start_of_week:       string;
}

export interface AcademicYearStart {
    day:   number;
    month: number;
}

export interface FiscalYearStart {
    corporate:  Corporate;
    government: AcademicYearStart;
    personal:   AcademicYearStart;
}

export interface Corporate {
    basis: string;
    day:   number;
    month: number;
}

export interface Demonyms {
    eng: Eng;
    fra: Eng;
}

export interface Eng {
    f: string;
    m: string;
}

export interface Economy {
    gini_coefficient: { [key: string]: number };
}

export interface Flag {
    colors:      Colors;
    description: string;
    emoji:       string;
    html_entity: string;
    unicode:     string;
    url_png:     string;
    url_svg:     string;
}

export interface Colors {
    dominant:  string;
    palette:   Palette[];
    prominent: string;
    swatches:  Swatches;
}

export interface Palette {
    hex:        string;
    proportion: number;
}

export interface Swatches {
    dark_muted:    null;
    dark_vibrant:  null;
    light_muted:   string;
    light_vibrant: string;
    muted:         string;
    vibrant:       string;
}

export interface Language {
    bcp47:       string;
    iso639_1:    string;
    iso639_2b:   string;
    iso639_2t:   string;
    iso639_3:    string;
    name:        string;
    native_name: string;
}

export interface Leader {
    message: string;
    sample:  string;
}

export interface Links {
    google_maps:      string;
    official:         string;
    open_street_maps: string;
    wikipedia:        string;
}

export interface Names {
    alternates:   string[];
    common:       string;
    native:       Native;
    official:     string;
    translations: { [key: string]: Jpn };
}

export interface Native {
    jpn: Jpn;
}

export interface Jpn {
    common:   string;
    official: string;
}

export interface NumberFormat {
    decimal_separator:   string;
    thousands_separator: string;
}

export interface Parent {
    alpha_2: string;
    alpha_3: string;
}

export interface PostalCode {
    format: string;
    regex:  string;
}
