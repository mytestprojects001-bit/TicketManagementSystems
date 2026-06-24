import { Component } from '@angular/core';
import { AuthService } from './auth.service';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-box" style="margin:50px auto; width:320px;">
      <div class="card card-outline card-primary">
        <div class="card-header text-center"><a href="#" class="h1"><b>Ticket</b>Admin</a></div>
        <div class="card-body">
          <p class="login-box-msg">Sign in to start your session</p>
          <form (ngSubmit)="onSubmit()">
            <div class="input-group mb-3">
              <input [(ngModel)]="model.userNameOrEmail" name="userNameOrEmail" class="form-control" placeholder="Username or email">
            </div>
            <div class="input-group mb-3">
              <input [(ngModel)]="model.password" name="password" type="password" class="form-control" placeholder="Password">
            </div>
            <div class="row">
              <div class="col-8"></div>
              <div class="col-4">
                <button class="btn btn-primary btn-block" type="submit">Sign In</button>
              </div>
            </div>
          </form>
          <p class="mt-3 mb-1"><a routerLink="/">Back to site</a></p>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  model: any = {};
  returnUrl: string | null = null;
  constructor(private auth: AuthService, private router: Router, private route: ActivatedRoute) {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
  }
  onSubmit() {
    this.auth.login(this.model).subscribe((res: any) => {
      if (res && res.data && res.data.token) {
        this.router.navigateByUrl(this.returnUrl || '/dashboard');
      }
    }, err => { alert(err.error?.message || 'Login failed'); });
  }
}
