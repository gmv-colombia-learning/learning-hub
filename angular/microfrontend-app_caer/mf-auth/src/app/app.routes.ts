import { Routes } from '@angular/router';
import { Profile } from './profile/profile';
import { Doraemon } from './doraemon/doraemon';

export const routes: Routes = [
  { path: '', component: Profile },
  { path: 'doraemon', component: Doraemon }
];
