import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { delay, map } from 'rxjs';

import type {
  User,
  UserResponse,
  UsersResponse,
} from '@interfaces/req-response';
import type { State } from '@interfaces/state';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private http = inject(HttpClient);

  #state = signal<State>({
    loading: true,
    users: [],
  });

  public users = computed(() => this.#state().users);
  public loading = computed(() => this.#state().loading);

  constructor() {
    this.http
      .get<UsersResponse>(environment.usersApi)
      .pipe(delay(1500))
      .subscribe((res) => {
        const users = res.data.map((user) => this.normalizeUser(user));

        this.#state.set({
          loading: false,
          users,
        });
      });
  }

  getUserById(id: string) {
    return this.http.get<UserResponse>(`${environment.usersApi}/${id}`).pipe(
      delay(1500),
      map((resp) => this.normalizeUser(resp.data)),
    );
  }

  private normalizeUser(user: User): User {
    const shouldUseFallbackAvatar = user.avatar?.includes(
      'reqres.in/img/faces',
    );

    if (!shouldUseFallbackAvatar) {
      return user;
    }

    return {
      ...user,
      avatar: `${environment.avatarUrl}${user.id}`,
    };
  }
}
