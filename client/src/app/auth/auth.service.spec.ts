import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule], providers: [AuthService] });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should call login and store tokens', () => {
    const mockResponse = { data: { token: 'abc', refreshToken: 'refresh' } };
    service.login({}).subscribe(res => {
      expect(res.data.token).toBe('abc');
    });
    const req = httpMock.expectOne((r) => r.url.endsWith('/api/auth/login'));
    req.flush(mockResponse);
    httpMock.verify();
  });
});
