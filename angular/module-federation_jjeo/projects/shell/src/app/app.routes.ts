import { Routes } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/native-federation';


export const routes: Routes = [
    {
        path: 'country',
        loadChildren: () =>
            loadRemoteModule('country', './Module')
            .then(m => m.routes)
    },

    {
        path: '',
        redirectTo: 'country',
        pathMatch: 'full'
    }
];
