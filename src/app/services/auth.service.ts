import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';

export interface User {
  maNguoiDung?: number;
  email?: string;
  hoTen: string;
  vaiTro?: string;
  soDienThoai?: string;
  ngaySinh?: string;
  gioiTinh?: number;
  diaChi?: string;
  anhDaiDien?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5186/api/v1/auth'; // Matches ASP.NET Core port from Program.cs
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('sh_user');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get token(): string | null {
    return localStorage.getItem('sh_token');
  }

  register(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, data);
  }

  login(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(res => this.handleAuthentication(res))
    );
  }

  getProfile(): Observable<User> {
    return this.http.get<User>('http://localhost:5186/api/v1/users/profile').pipe(
      tap((user: User) => {
        if (user) {
          const current = this.currentUserSubject.value;
          const updatedUser: User = {
            ...current,
            ...user
          };
          localStorage.setItem('sh_user', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
        }
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('sh_refresh_token');
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(res => this.handleAuthentication(res))
    );
  }

  logout(): Observable<any> {
    const refreshToken = localStorage.getItem('sh_refresh_token');
    return this.http.post(`${this.apiUrl}/logout`, { refreshToken }).pipe(
      tap(() => {
        this.clearSession();
      })
    );
  }

  verifyOtp(email: string, code: string, loaiOTP: string = 'Register'): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-otp`, { email, code, loaiOTP });
  }

  resendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-otp`, { email });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }

  confirmEmail(email: string, token: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/confirm-email?email=${encodeURIComponent(email)}&token=${encodeURIComponent(token)}`);
  }

  private handleAuthentication(response: AuthResponse) {
    localStorage.setItem('sh_token', response.accessToken);
    localStorage.setItem('sh_refresh_token', response.refreshToken);
    localStorage.setItem('sh_user', JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }

  public clearSession() {
    localStorage.removeItem('sh_token');
    localStorage.removeItem('sh_refresh_token');
    localStorage.removeItem('sh_user');
    this.currentUserSubject.next(null);
  }
}
