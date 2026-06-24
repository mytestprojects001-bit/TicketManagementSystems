import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { TokenStorage } from './token-storage';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private refreshInProgress = false;
  private refreshSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  constructor(private auth: AuthService, private router: Router) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.getToken();
    let authReq = req;
    if (token) {
      authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }

    return next.handle(authReq).pipe(catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        const refreshToken = this.auth.getRefreshToken();
        if (!refreshToken) {
          this.handleLogout();
          return throwError(() => err);
        }

        if (!this.refreshInProgress) {
          this.refreshInProgress = true;
          this.refreshSubject.next(null);

          return this.auth.refresh(refreshToken).pipe(
            switchMap((res: any) => {
              this.refreshInProgress = false;
              const newToken = res && res.data && res.data.token ? res.data.token : null;
              this.refreshSubject.next(newToken);
              const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
              return next.handle(retryReq);
            }),
            catchError(refreshErr => {
              this.refreshInProgress = false;
              this.handleLogout();
              return throwError(() => refreshErr || err);
            })
          );
        } else {
          return this.refreshSubject.pipe(
            filter(tokenVal => tokenVal != null),
            take(1),
            switchMap(tokenVal => {
              const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${tokenVal}` } });
              return next.handle(retryReq);
            })
          );
        }
      }
      return throwError(() => err);
    }));
  }

  private handleLogout() {
    TokenStorage.clear();
    this.router.navigate(['/login']);
  }
}
