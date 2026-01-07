import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { LoginRequestDto } from '../models/login-request-dto';
import { LoginTokenDto } from '../models/login-token-dto';

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  constructor(private http: HttpClient) {}

  login(dto: LoginRequestDto): Observable<string> {
    return this.http.post<LoginTokenDto>('/api/auth/login', dto).pipe(
      map(res => res.token)
    );
  }
  
}
