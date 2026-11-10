import { Routes } from '@angular/router';
import { countryRoutes } from './country/country.routes';
import { HomePageComponent } from './shared/pages/home-pages/home-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomePageComponent,
    pathMatch: 'full',
  },
  ...countryRoutes,
];
