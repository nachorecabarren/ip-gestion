import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ConfirmService } from '../../shared/services/confirm.service';
import { TeamService, TeamUser, PendingInvitation, InvitationLink } from '../../core/services/team.service';

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

  users = signal<TeamUser[]>([]);
  invites = signal<PendingInvitation[]>([]);
  loading = signal(true);

  inviting = signal(false);
  inviteError = signal<string | null>(null);
  lastLink = signal<InvitationLink | null>(null);
  showLastLink = signal(false);
  copiedId = signal<string | null>(null);

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
    this.showLastLink.set(false);

    const email = this.form.value.email!;
    this.team.createInvitation(email).subscribe({
      next: (link) => {
        this.inviting.set(false);
        this.lastLink.set(link);
        this.form.reset();
        this.load();
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
    return role === 'OWNER' ? 'Dueño' : 'Empleado';
  }
}
