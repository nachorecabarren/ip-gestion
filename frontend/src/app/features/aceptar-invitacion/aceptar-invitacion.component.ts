import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { TeamService, InvitationInfo } from '../../core/services/team.service';

@Component({
  selector: 'app-aceptar-invitacion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './aceptar-invitacion.component.html',
  styleUrls: ['./aceptar-invitacion.component.scss']
})
export class AceptarInvitacionComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private team = inject(TeamService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  private token = '';

  validating = signal(true);
  info = signal<InvitationInfo | null>(null);
  invalidToken = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);
  showPassword = signal(false);

  form = this.fb.group({
    displayName: ['', [Validators.required, Validators.minLength(2)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) { this.invalidToken.set(true); this.validating.set(false); return; }

    this.team.validateInvitation(this.token).subscribe({
      next: (info) => { this.info.set(info); this.validating.set(false); },
      error: () => { this.invalidToken.set(true); this.validating.set(false); }
    });
  }

  invalid(name: string): boolean {
    const c = this.form.get(name);
    return !!c && c.invalid && c.touched;
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);

    const v = this.form.value;
    this.team.acceptInvitation(this.token, v.displayName!, v.password!).subscribe({
      next: (user) => { this.auth.applyUser(user); this.router.navigate(['/dashboard']); },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.error ?? 'No se pudo aceptar la invitación');
      }
    });
  }
}
