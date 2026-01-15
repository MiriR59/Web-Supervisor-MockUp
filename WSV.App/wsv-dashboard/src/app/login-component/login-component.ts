import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LoginService } from '../services/login-service';
import { AuthService } from '../services/auth-service';
import { RefreshService } from '../services/refresh-service';
import { ɵEmptyOutletComponent } from "@angular/router";

@Component({
  selector: 'app-login-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login-component.html',
  styleUrl: './login-component.css',
})
export class LoginComponent {
  loading = signal(false);
  errorMessage = signal<string | null>(null);

  login = {
    userName: '',
    password: ''
  }

  constructor(
    private loginService: LoginService,
    public authService: AuthService,
  ) {}

  submit() {
    if(this.loading())
      return;

    this.loading.set(true);

    this.loginService.login(this.login).subscribe({
      next: (token: string) => {
        this.authService.setAuth(token);
        this.login = { userName: '', password: '' };
        this.errorMessage.set(null);
        this.loading.set(false);
      },

      error: (err) => {
        this.loading.set(false);
        this.login.password = '';

        if(err.status === 401) {
          this.errorMessage.set('Invalid username or password.');
        }
        else {
          this.errorMessage.set('Login failed.')
        }
      }

    })
  }

  logout() {
    this.authService.clearAuth();
  }
}
