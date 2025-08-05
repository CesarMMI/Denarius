import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LoginRequest } from '../types/login.request';
import { environment } from '../../../environments/environment';
import { SkipAuthContext } from '../../core/context/skip-auth.context';
import { AuthResponse } from '../types/auth.response';
import { AuthService } from '../../core/services/auth.service';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PublicService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private baseUrl = `${environment.apiUrl}/auth`;
  private options = { context: new SkipAuthContext() };

  login(req: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, req, this.options)
      .pipe(tap((res) => this.authService.login(res.user, res.accessToken, res.refreshToken)));
  }
}
