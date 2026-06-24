import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private base = environment.apiBaseUrl + '/api/dashboard';
  constructor(private http: HttpClient) {}

  getSummary(): Observable<any> {
    return this.http.get(this.base + '/summary');
  }

  getRecentBookings(): Observable<any> {
    return this.http.get(environment.apiBaseUrl + '/api/bookings/recent');
  }
}
