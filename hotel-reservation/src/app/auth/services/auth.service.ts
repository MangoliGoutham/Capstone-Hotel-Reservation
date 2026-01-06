import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginDto, RegisterDto, AuthResponseDto, User } from '../../shared/models/auth.models';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/Auth';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    if (typeof localStorage !== 'undefined') {
      const user = localStorage.getItem('user');
      const token = localStorage.getItem('token');
      if (user && token) {
        this.currentUserSubject.next(JSON.parse(user));
      }
    }
  }

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.handleAuthResponse(response);
      })
    );
  }

  register(data: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/register`, data).pipe(
      tap(response => {
        this.handleAuthResponse(response);
      })
    );
  }

  private handleAuthResponse(response: AuthResponseDto): void {
    localStorage.setItem('token', response.token);
    const user: User = {
      firstName: response.firstName,
      lastName: response.lastName,
      email: '',
      role: response.role,
      hotelId: response.hotelId
    };
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
