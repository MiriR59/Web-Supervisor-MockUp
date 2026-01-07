import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LoginService } from '../services/login-service';
import { AuthService } from '../services/auth-service';

@Component({
  selector: 'app-login-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login-component.html',
  styleUrl: './login-component.css',
})
export class LoginComponent {
  loading = false;
  submitted = false;

  login = {
    userName: '',
    password: ''
  }

  constructor(
    private loginService: LoginService,
    private authService: AuthService
  ) {}

  submit() {
    if(this.loading)
      return;

    this.loading = true;
    this.loginService.login(this.login).subscribe({
      next: (token: string) => {
        this.authService.setToken(token);
        this.submitted = true;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }

    })
  }
}
