import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LoginService } from '../services/login-service';
import { AuthService } from '../services/auth-service';
import { RefreshService } from '../services/refresh-service';

@Component({
  selector: 'app-login-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login-component.html',
  styleUrl: './login-component.css',
})
export class LoginComponent {
  loading = signal(false);

  login = {
    userName: '',
    password: ''
  }

  constructor(
    private loginService: LoginService,
    public authService: AuthService,
    private refreshService: RefreshService,
  ) {}

  submit() {
    if(this.loading())
      return;

    this.loading.set(true);

    this.loginService.login(this.login).subscribe({
      next: (token: string) => {
        this.authService.setToken(token);
        this.loading.set(false);
      },

      error: (err) => {
        this.loading.set(false);
      }

    })
  }
}
