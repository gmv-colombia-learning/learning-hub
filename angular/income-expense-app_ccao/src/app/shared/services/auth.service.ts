import { Injectable, OnDestroy } from '@angular/core';
import {
  Auth,
  createUserWithEmailAndPassword,
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signOut,
  User as UserAuth,
} from '@angular/fire/auth';
import { Observable, Subscription, from } from 'rxjs';
import { Firestore, doc, setDoc, docData } from '@angular/fire/firestore';
import { authState } from '@angular/fire/auth';

import { Store } from '@ngrx/store';
import * as authActions from '../../pages/auth/auth.actions';
import * as incomeExpenseActions from '../../pages/income-expense/income-expense.actions';

import { map } from 'rxjs/operators';
import { User, userFromFirebase } from '../models/user.model';
import { AppState } from '../../app.reducer';

@Injectable({
  providedIn: 'root',
})
export class AuthService implements OnDestroy {
  private userSubscription!: Subscription;
  private _user!: User | null;

  get user() {
    return this._user;
  }

  constructor(
    private auth: Auth,
    private firestore: Firestore,
    private store: Store<AppState>,
  ) {}

  ngOnDestroy(): void {
    this.userSubscription?.unsubscribe();
  }

  public initAuthListener(): void {
    onAuthStateChanged(this.auth, (fuser: UserAuth | null) => {
      if (fuser) {
        const userDocRef = doc(this.firestore, `users/${fuser.uid}`);
        try {
          this.userSubscription = docData(userDocRef).subscribe(
            (firestoreUser: any) => {
              if (firestoreUser) {
                this._user = userFromFirebase(firestoreUser);
                this.store.dispatch(authActions.setUser({ user: this._user }));
              }
            },
          );
        } catch (error) {
          console.error('Error fetching user data:', error);
        }
      } else {
        this._user = null;
        this.userSubscription?.unsubscribe();
        this.store.dispatch(authActions.unSetUser());
        this.store.dispatch(incomeExpenseActions.unSetItems());
      }
    });
  }

  public async createUser(
    name: string,
    email: string,
    password: string,
  ): Promise<User> {
    const credential = await createUserWithEmailAndPassword(
      this.auth,
      email,
      password,
    );

    const user: UserAuth = credential.user;
    const newUser: User = { uid: user.uid, name, email: user.email };
    const userRef = doc(this.firestore, `users/${user.uid}`);

    await setDoc(userRef, newUser);

    return newUser;
  }

  public login(email: string, password: string): Observable<UserAuth> {
    return from(
      signInWithEmailAndPassword(this.auth, email, password).then(
        (res) => res.user,
      ),
    );
  }

  public logout(): Observable<void> {
    return from(signOut(this.auth));
  }

  public isAuth(): Observable<boolean> {
    return authState(this.auth).pipe(map((user) => user != null));
  }
}
