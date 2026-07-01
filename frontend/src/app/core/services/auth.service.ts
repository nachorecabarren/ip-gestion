import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuthUser {
  userId: string;
  tenantId: string;
  email: string;
  displayName: string;
  role: 'OWNER' | 'EMPLOYEE' | string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private base = `${environment.apiUrl}/auth`;

  /** null = logged out, undefined-ish via `checked` flag = not yet resolved */
  currentUser = signal<AuthUser | null>(null);
  checked = signal(false);

  isOwner = computed(() => this.currentUser()?.role === 'OWNER');
  isLoggedIn = computed(() => this.currentUser() !== null);

  /** Called once on app start (and by the guard) to restore the session from the cookie. */
  checkSession(): Observable<boolean> {
    if (this.checked()) return of(this.currentUser() !== null);
    return this.http.get<AuthUser>(`${this.base}/me`).pipe(
      tap(u => { this.currentUser.set(u); this.checked.set(true); }),
      map(() => true),
      catchError(() => { this.currentUser.set(null); this.checked.set(true); return of(false); })
    );
  }

  login(email: string, password: string): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${this.base}/login`, { email, password }).pipe(
      tap(u => { this.currentUser.set(u); this.checked.set(true); })
    );
  }

  register(businessName: string, ownerEmail: string, ownerPassword: string, ownerDisplayName: string): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${this.base}/register`,
      { businessName, ownerEmail, ownerPassword, ownerDisplayName }).pipe(
      tap(u => { this.currentUser.set(u); this.checked.set(true); })
    );
  }

  /** Used after accepting an invitation (the accept endpoint also sets the cookie). */
  applyUser(user: AuthUser): void {
    this.currentUser.set(user);
    this.checked.set(true);
  }

  logout(): void {
    this.http.post<void>(`${this.base}/logout`, {}).pipe(
      catchError(() => of(void 0))
    ).subscribe(() => {
      this.currentUser.set(null);
      this.checked.set(true);
      this.router.navigate(['/login']);
    });
  }
}
