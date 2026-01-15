import { Injectable, signal, computed } from '@angular/core';
import { decodeJwtPayload, getRole, getUserName, isExpired } from '../auth/jwt-decoder';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'wsv_token'
  private logoutTimerId: number | null = null;

  private readonly _token = signal<string | null>(localStorage.getItem(this.tokenKey))
  private readonly _username = signal<string | null>(null);
  private readonly _role = signal<string | null>(null);
  private readonly _expiresAt = signal<number | null>(null);
  
  // computed creates reactive signal from given expression
  readonly token = computed(() => this._token());
  readonly username = computed(() => this._username());
  readonly role = computed(() => this._role());
  readonly expiresAt = computed(() => this._expiresAt());

  readonly isLoggedIn = computed(() =>
    this._token() !==null && this._expiresAt() !== null && Date.now() < this._expiresAt()!);

  readonly canToggleSources = computed(() => {
    const r = this._role();
    return r === 'Admin' || r === 'Operator';
  });

  // Restores username, role and timer if expiration is OK
  constructor() {
    // Token gets pulled from local, doesnt erase on reload as other signals
    const  t = this._token();
    if (t) {
      const ok = this.applyToken(t);
      if (!ok)
        this.clearAuth();
    }
  }

  setAuth(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this._token.set(token);

    const ok = this.applyToken(token);
    if (!ok)
      this.clearAuth();
  }

  clearAuth(): void {
    this.cancelLogoutTimer();

    localStorage.removeItem(this.tokenKey);
    this._token.set(null);
    this._username.set(null);
    this._role.set(null);
    this._expiresAt.set(null);
  }

  // Definition of internal methods

  private applyToken(token: string): boolean {
    const payload = decodeJwtPayload(token);
    if (!payload)
      return false;

    if (isExpired(payload))
      return false;

    const username = getUserName(payload);
    const role = getRole(payload);
    this._username.set(username);
    this._role.set(role);

    const expiresSeconds: number | undefined = payload?.exp;
    if (typeof expiresSeconds !== 'number')
      return false;

    const expiresAt = expiresSeconds * 1000;
    this._expiresAt.set(expiresAt);

    this.scheduleLogoutTimer(expiresAt);
    return true;
  }

  private scheduleLogoutTimer(expiresAt: number): void {
    this.cancelLogoutTimer();

    const now = Date.now();
    const remains = expiresAt - now;

    if (remains <= 0) {
      this.clearAuth();
      return;
    }

    // Logout early to avoid edge cases
    const shift = 10_000;
    const LogoutIn = Math.max(0, remains - shift);

    this.logoutTimerId = window.setTimeout(() => {
      this.clearAuth();
    },
      LogoutIn);
  }

  private cancelLogoutTimer(): void {
    if (this.logoutTimerId !== null) {
      clearTimeout(this.logoutTimerId);
      this.logoutTimerId = null;
    }
  }
}
