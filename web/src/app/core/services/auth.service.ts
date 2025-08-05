import { computed, Injectable, signal } from '@angular/core';
import { User } from '../types/user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _user = signal<User | undefined>(undefined);
  private _accessToken = signal<string | undefined>(undefined);
  private _refreshToken = signal<string | undefined>(undefined);

  readonly user = this._user.asReadonly();
  readonly accessToken = this._accessToken.asReadonly();
  readonly refreshToken = this._refreshToken.asReadonly();

  readonly authenticated = computed(() => !!this.user() && !!this.accessToken() && !!this.refreshToken());

  login(user: User, accessToken: string, refreshToken: string) {
    this._user.set(user);
    this._accessToken.set(accessToken);
    this._refreshToken.set(refreshToken);
  }

  logout() {
    this._user.set(undefined);
    this._accessToken.set(undefined);
    this._refreshToken.set(undefined);
  }

  refresh(user: User, accessToken: string) {
    this._user.set(user);
    this._accessToken.set(accessToken);
  }
}
