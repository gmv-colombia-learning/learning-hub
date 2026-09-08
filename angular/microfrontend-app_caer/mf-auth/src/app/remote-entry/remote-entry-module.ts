import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Profile } from '../profile/profile';
import { Doraemon } from '../doraemon/doraemon';


@NgModule({
  declarations: [Profile],
  imports: [
    RouterModule.forChild([
      { path: '', component: Profile },
      { path: 'doraemon', component: Doraemon }
    ])
  ]
})
export class RemoteEntryModule { }
