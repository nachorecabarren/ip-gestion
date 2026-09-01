import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { RetentionRule, RetentionTouchpoint } from '../../shared/models/models';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-retencion',
  standalone: true,
  imports: [CommonModule, PaginationComponent],
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
  page = signal(1);
  pageSize = signal(100);
  total = signal(0);
  totalAll = signal(0);
  paraHoy = signal(0);
  vencidos = signal(0);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.api.getTouchpoints(this.filterStatus() || undefined, this.page(), this.pageSize()).subscribe({
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

  onPageParams(p: PageParams) {
    this.page.set(p.page);
    this.pageSize.set(p.pageSize);
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
}
