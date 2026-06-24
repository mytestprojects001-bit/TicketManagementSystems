import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService } from './auth.service';

describe('AuthInterceptor', () => {
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [
        AuthService,
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
      ]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should attach token when present', () => {
    localStorage.setItem('tm_token', 'xyz');
    const http = TestBed.inject(HttpClient);
    http.get('/api/protected').subscribe();
    const req = httpMock.expectOne('/api/protected');
    expect(req.request.headers.has('Authorization')).toBeTrue();
    req.flush({});
    httpMock.verify();
    localStorage.removeItem('tm_token');
  });
});
