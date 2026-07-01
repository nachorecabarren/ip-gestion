import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { RetentionRule, RetentionTouchpoint } from '../../shared/models/models';

@Component({
  selector: 'app-retencion',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './retencion.component.html',
  styleUrls: ['./retencion.component.scss']
})
export class RetencionComponent implements OnInit {
  private api = inject(ApiService);
  tab = signal<'touchpoints' | 'reglas'>('touchpoints');
  touchpoints = signal<RetentionTouchpoint[]>([]);
  rules = signal<RetentionRule[]>([]);
  loading = signal(true);
  filterStatus = signal('');

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.api.getTouchpoints(this.filterStatus() || undefined).subscribe({
      next: t => { this.touchpoints.set(t); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.api.getRetentionRules().subscribe(r => this.rules.set(r));
  }

  get paraHoy() { return this.touchpoints().filter(t => t.status === 'PARA_HOY').length; }
  get vencidos() { return this.touchpoints().filter(t => t.status === 'VENCIDO').length; }

  openWhatsapp(phone: string, message: string) {
    const clean = phone.replace(/\D/g, '');
    const encoded = encodeURIComponent(message);
    window.open(`https://wa.me/${clean}?text=${encoded}`, '_blank');
  }

  copyMessage(message: string) {
    navigator.clipboard.writeText(message);
  }

  getStatusClass(s: string) {
    return ({ PARA_HOY: 'badge--amber', VENCIDO: 'badge--red', PENDIENTE: 'badge--gray' })[s] ?? 'badge--gray';
  }
  getStatusLabel(s: string) {
    return ({ PARA_HOY: 'Para hoy', VENCIDO: 'Vencido', PENDIENTE: 'Pendiente' })[s] ?? s;
  }
}