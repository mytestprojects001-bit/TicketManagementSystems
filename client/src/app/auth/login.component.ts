import { Component } from '@angular/core';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div>
      <h3>Login</h3>
      <form (ngSubmit)="onSubmit()">
        <div>
          <label>Username or Email</label>
          <input [(ngModel)]="model.userNameOrEmail" name="userNameOrEmail" />
        </div>
        <div>
          <label>Password</label>
          <input type="password" [(ngModel)]="model.password" name="password" />
        </div>
        <button type="submit">Login</button>
      </form>
      <div *ngIf="result">{{result | json}}</div>
    </div>
  `
})
export class LoginComponent {
  model: any = {};
  result: any;
  constructor(private auth: AuthService) {}
  onSubmit() {
    this.auth.login(this.model).subscribe(res => this.result = res, err => this.result = err.error);
  }
}
