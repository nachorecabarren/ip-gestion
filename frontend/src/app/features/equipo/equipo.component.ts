import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ConfirmService } from '../../shared/services/confirm.service';
import { AuthService } from '../../core/services/auth.service';
import { TeamService, TeamUser, PendingInvitation, InvitationLink, ASSIGNABLE_ROLES, AssignableRole } from '../../core/services/team.service';

@Component({
  selector: 'app-equipo',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './equipo.component.html',
  styleUrls: ['./equipo.component.scss']
})
export class EquipoComponent implements OnInit {
  private team = inject(TeamService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  auth = inject(AuthService);

  users = signal<TeamUser[]>([]);
  invites = signal<PendingInvitation[]>([]);
  loading = signal(true);

  inviting = signal(false);
  inviteError = signal<string | null>(null);
  lastLink = signal<InvitationLink | null>(null);
  copiedId = signal<string | null>(null);

  assignableRoles = ASSIGNABLE_ROLES;
  roleChangingId = signal<string | null>(null);
  roleError = signal<string | null>(null);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.team.getUsers().subscribe({
      next: u => { this.users.set(u); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.team.getPendingInvitations().subscribe({
      next: i => this.invites.set(i),
      error: () => {}
    });
  }

  invite() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.inviting.set(true);
    this.inviteError.set(null);
    this.lastLink.set(null);

    const email = this.form.value.email!;
    this.team.createInvitation(email).subscribe({
      next: (link) => {
        this.inviting.set(false);
        this.lastLink.set(link);
        this.form.reset();
        this.load();
        setTimeout(() => this.lastLink.set(null), 5000);
      },
      error: (err) => {
        this.inviting.set(false);
        this.inviteError.set(err?.error?.error ?? 'No se pudo crear la invitación');
      }
    });
  }

  buildLink(inv: PendingInvitation): string {
    return `${window.location.origin}/aceptar-invitacion?token=${inv.token}`;
  }

  copyLink(url: string, id: string) {
    navigator.clipboard?.writeText(url).then(() => {
      this.copiedId.set(id);
      setTimeout(() => this.copiedId.set(null), 2000);
    });
  }

  async cancelInvite(inv: PendingInvitation) {
    if (!await this.confirm.open(`¿Cancelar la invitación a ${inv.email}?`)) return;
    this.team.cancelInvitation(inv.id).subscribe(() => this.load());
  }

  async deactivate(user: TeamUser) {
    if (!await this.confirm.open(`¿Desactivar a ${user.displayName}? Perderá el acceso al sistema.`)) return;
    this.team.deactivateUser(user.id).subscribe(() => this.load());
  }

  roleLabel(role: string) {
    return ({
      OWNER: 'Dueño', ADMIN: 'Administrador', OPERATOR: 'Operador', VIEWER: 'Solo lectura', EMPLOYEE: 'Empleado',
    } as Record<string, string>)[role] ?? role;
  }

  canChangeRole(user: TeamUser): boolean {
    return this.auth.canManageRoles() && user.role !== 'OWNER' && user.id !== this.auth.currentUser()?.userId;
  }

  async changeRole(user: TeamUser, newRole: string) {
    const role = newRole as AssignableRole;
    if (role === user.role) return;
    if (!await this.confirm.open(`¿Cambiar el rol de ${user.displayName} a "${this.roleLabel(role)}"?`)) return;

    this.roleError.set(null);
    this.roleChangingId.set(user.id);
    this.team.updateRole(user.id, role).subscribe({
      next: () => { this.roleChangingId.set(null); this.load(); },
      error: (err) => {
        this.roleChangingId.set(null);
        this.roleError.set(err?.error?.error ?? 'No se pudo cambiar el rol');
      },
    });
  }
}
