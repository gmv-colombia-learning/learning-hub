import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { provideStore } from '@ngrx/store';

import { routes } from './app/app-routing.module';

import { provideFirebaseApp, initializeApp } from '@angular/fire/app';
import { provideFirestore, getFirestore } from '@angular/fire/firestore';
import { provideAuth, getAuth } from '@angular/fire/auth';

import { environment } from './environments/environment';
import { appReducers } from './app/app.reducer';


bootstrapApplication(AppComponent, {
  providers: [
    provideStore(appReducers),
    provideRouter(routes),

    provideFirebaseApp(() => initializeApp(environment.firebase)),
    provideFirestore(() => getFirestore()),
    provideAuth(() => getAuth()),
  ]
});
