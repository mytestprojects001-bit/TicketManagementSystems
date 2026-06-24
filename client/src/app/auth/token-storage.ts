export const TOKEN_KEY = 'tm_token';
export const REFRESH_KEY = 'tm_refresh';

export class TokenStorage {
  static saveToken(token: string) { localStorage.setItem(TOKEN_KEY, token); }
  static getToken(): string | null { return localStorage.getItem(TOKEN_KEY); }
  static removeToken() { localStorage.removeItem(TOKEN_KEY); }

  static saveRefreshToken(token: string) { localStorage.setItem(REFRESH_KEY, token); }
  static getRefreshToken(): string | null { return localStorage.getItem(REFRESH_KEY); }
  static removeRefreshToken() { localStorage.removeItem(REFRESH_KEY); }

  static clear() { localStorage.removeItem(TOKEN_KEY); localStorage.removeItem(REFRESH_KEY); }
}
