import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthService {
  baseUrl = '/api/auth';
  token: string | null = null;
  constructor(private http: HttpClient) {}

  login(model: any): Observable<any> {
    return this.http.post(this.baseUrl + '/login', model);
  }

  register(model: any): Observable<any> {
    return this.http.post(this.baseUrl + '/register', model);
  }
}
