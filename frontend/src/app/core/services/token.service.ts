import { Injectable } from '@angular/core';
import { AuthResponse, UserDto } from '../../shared/models/auth.model';

const TOKEN_KEY = 'dw_token';
const USER_KEY = 'dw_user';

export type StoredUser = UserDto;

@Injectable({ providedIn: 'root' })
export class TokenService {
  setTokens(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUser(): StoredUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as StoredUser;
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  isAdmin(): boolean {
    return this.getUser()?.role === 'Admin';
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
