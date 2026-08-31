import { Routes } from '@angular/router';
import { authGuard } from './features/user/infrastructure/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/user/user.routes').then((routes) => routes.USER_ROUTES),
  },
  {
    path: '',
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    loadComponent: () =>
      import('./layouts/private-layout/private-layout/private-layout').then(
        (component) => component.PrivateLayout,
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboard/ui/pages/dashboard-page/dashboard-page').then(
            (component) => component.DashboardPage,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
