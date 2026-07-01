import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser } from './auth.service';

export interface InvitationLink {
  id: string;
  email: string;
  token: string;
  acceptUrl: string;
  expiresAt: string;
}
export interface InvitationInfo { email: string; businessName: string; }
export interface PendingInvitation {
  id: string;
  email: string;
  token: string;
  createdAt: string;
  expiresAt: string;
}
export interface TeamUser {
  id: string;
  email: string;
  displayName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class TeamService {
  private http = inject(HttpClient);
  private inv = `${environment.apiUrl}/invitations`;
  private users = `${environment.apiUrl}/usuarios`;

  // ─── Invitations (OWNER) ─────────────────────────────
  createInvitation(email: string): Observable<InvitationLink> {
    return this.http.post<InvitationLink>(this.inv, { email });
  }
  getPendingInvitations(): Observable<PendingInvitation[]> {
    return this.http.get<PendingInvitation[]>(this.inv);
  }
  cancelInvitation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.inv}/${id}`);
  }

  // ─── Invitation acceptance (anonymous) ───────────────
  validateInvitation(token: string): Observable<InvitationInfo> {
    return this.http.get<InvitationInfo>(`${this.inv}/validate/${token}`);
  }
  acceptInvitation(token: string, displayName: string, password: string): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${this.inv}/accept`, { token, displayName, password });
  }

  // ─── Team users (OWNER) ──────────────────────────────
  getUsers(): Observable<TeamUser[]> {
    return this.http.get<TeamUser[]>(this.users);
  }
  deactivateUser(id: string): Observable<void> {
    return this.http.put<void>(`${this.users}/${id}/desactivar`, {});
  }
}
