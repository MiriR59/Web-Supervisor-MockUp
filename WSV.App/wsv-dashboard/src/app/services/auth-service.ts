import { Injectable, signal, computed } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'wsv_token'
  private readonly _token = signal<string | null>(localStorage.getItem(this.tokenKey))
  // computed creates reactive signal from given expression
  readonly token = computed(() => this._token())
  readonly isLoggedIn = computed(() => !!this._token())

  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this._token.set(token);
  }

  clearToken(): void {
    localStorage.removeItem(this.tokenKey);
    this._token.set(null);
  }
}
