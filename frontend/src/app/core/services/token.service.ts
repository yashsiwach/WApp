import { Injectable } from '@angular/core';
import { AuthResponse, UserDto } from '../../shared/models/auth.model';

const TOKEN_KEY = 'dw_token';
const USER_KEY = 'dw_user';

export type StoredUser = UserDto;

@Injectable({ providedIn: 'root' })
export class TokenService {
  // Persist the authenticated session returned by login or signup.
  setTokens(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
  }

  // Read the current access token from local storage.
  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  // Read and parse the stored user profile if one exists.
  getUser(): StoredUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as StoredUser;
    } catch {
      return null;
    }
  }

  // Treat any stored access token as an active session.
  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  // Use the stored user role to decide whether admin-only routes should be enabled.
  isAdmin(): boolean {
    return this.getUser()?.role === 'Admin';
  }

  // Remove all persisted auth state during logout or unauthorized responses.
  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
