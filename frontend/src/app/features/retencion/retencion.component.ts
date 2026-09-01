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
  readonly pageSize = 25;
  tab = signal<'touchpoints' | 'reglas'>('touchpoints');
  touchpoints = signal<RetentionTouchpoint[]>([]);
  rules = signal<RetentionRule[]>([]);
  loading = signal(true);
  filterStatus = signal('');
  page = signal(1);
  total = signal(0);
  totalAll = signal(0);
  paraHoy = signal(0);
  vencidos = signal(0);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.api.getTouchpoints(this.filterStatus() || undefined, this.page(), this.pageSize).subscribe({
      next: r => {
        this.touchpoints.set(r.items);
        this.total.set(r.total);
        this.totalAll.set(r.totalAll);
        this.paraHoy.set(r.paraHoyCount);
        this.vencidos.set(r.vencidoCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.api.getRetentionRules().subscribe(r => this.rules.set(r));
  }

  setFilter(status: string) {
    this.filterStatus.set(status);
    this.page.set(1);
    this.load();
  }

  prevPage() {
    if (this.page() <= 1) return;
    this.page.set(this.page() - 1);
    this.load();
  }

  nextPage() {
    if (this.page() * this.pageSize >= this.total()) return;
    this.page.set(this.page() + 1);
    this.load();
  }

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

  ceilDiv(total: number, size: number) {
    return Math.max(1, Math.ceil(total / size));
  }
}