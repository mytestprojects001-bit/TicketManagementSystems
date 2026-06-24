import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TokenStorage } from './token-storage';

@Injectable()
export class AuthService {
  private base = environment.apiBaseUrl + '/api/auth';
  constructor(private http: HttpClient) { }

  login(model: any): Observable<any> {
    return this.http.post<any>(this.base + '/login', model).pipe(
      tap(res => {
        if (res && res.data) {
          if (res.data.token) TokenStorage.saveToken(res.data.token);
          if (res.data.refreshToken) TokenStorage.saveRefreshToken(res.data.refreshToken);
        }
      })
    );
  }

  refresh(refreshToken: string): Observable<any> {
    if (!refreshToken) return of(null as any);
    return this.http.post<any>(this.base + '/refresh', { refreshToken }).pipe(
      tap(res => {
        if (res && res.data) {
          if (res.data.token) TokenStorage.saveToken(res.data.token);
          if (res.data.refreshToken) TokenStorage.saveRefreshToken(res.data.refreshToken);
        }
      })
    );
  }

  logout() {
    TokenStorage.clear();
    // Optionally call backend revoke endpoint if implemented
  }

  getToken(): string | null {
    return TokenStorage.getToken();
  }

  getRefreshToken(): string | null {
    return TokenStorage.getRefreshToken();
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
