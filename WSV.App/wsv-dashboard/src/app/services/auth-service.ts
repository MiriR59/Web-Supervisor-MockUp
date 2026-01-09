import { Injectable, signal, computed } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'wsv_token'
  private readonly usernameKey = 'wsv_username'

  private readonly _token = signal<string | null>(localStorage.getItem(this.tokenKey))
  private readonly _username = signal<string | null>(null);
  
  // computed creates reactive signal from given expression
  readonly token = computed(() => this._token())
  readonly isLoggedIn = computed(() => !!this._token())
  readonly username = computed(() => this._username());

  setAuth(token: string, username: string): void {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(this.usernameKey, username);

    this._token.set(token);
    this._username.set(username);
  }

  clearAuth(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.usernameKey);

    this._token.set(null);
    this._username.set(null);
  }
}
