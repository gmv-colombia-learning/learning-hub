import { Region } from '../../country/interfaces/region.type';

export const validRegions: Record<string, Region> = {
  africa: 'Africa',
  americas: 'Americas',
  asia: 'Asia',
  europe: 'Europe',
  oceania: 'Oceania',
  antarctic: 'Antarctic',
};

export function validateQueryParam(queryParam: string): Region {
  queryParam = queryParam.toLowerCase();
  return validRegions[queryParam] ?? 'Americas';
}
