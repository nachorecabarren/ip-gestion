import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { Sale, CashMovement } from '../../shared/models/models';

@Component({
  selector: 'app-base-datos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './base-datos.component.html',
  styleUrls: ['./base-datos.component.scss']
})
export class BaseDatosComponent implements OnInit {
  private api = inject(ApiService);

  activeTab = signal<'caja'|'minorista'|'mayorista'|'sv'|'reservas'>('caja');
  movements = signal<CashMovement[]>([]);
  sales = signal<Sale[]>([]);
  loading = signal(true);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.api.getCashMovements().subscribe(m => { this.movements.set(m); this.loading.set(false); });
    this.api.getSales().subscribe(r => this.sales.set(r.items));
  }

  get retailSales() { return this.sales().filter(s => s.category === 'RETAIL' && s.status === 'COMPLETED'); }
  get wholesaleSales() { return this.sales().filter(s => s.category === 'WHOLESALE' && s.status === 'COMPLETED'); }
  get totalRetail() { return this.retailSales.reduce((s, x) => s + x.totalUsd, 0); }
  get totalWholesale() { return this.wholesaleSales.reduce((s, x) => s + x.totalUsd, 0); }

  exportExcel() {
    const tab = this.activeTab();
    let headers: string[];
    let rows: (string | number)[][];

    if (tab === 'caja') {
      headers = ['Fecha', 'Tipo', 'Método', 'Caja', 'Detalle', 'Monto USD'];
      rows = this.movements().map(m => [
        new Date(m.createdAt).toLocaleDateString('es-AR'),
        m.type, m.method, m.cajaName, m.detail ?? '', m.amountUsd
      ]);
    } else if (tab === 'minorista') {
      headers = ['Fecha', 'Cliente', 'Total USD', 'Margen USD'];
      rows = this.retailSales.map(s => [
        new Date(s.saleDate).toLocaleDateString('es-AR'),
        s.clientName ?? 'CF', s.totalUsd, s.marginUsd
      ]);
    } else if (tab === 'mayorista') {
      headers = ['Fecha', 'Cliente', 'Total USD', 'Margen USD'];
      rows = this.wholesaleSales.map(s => [
        new Date(s.saleDate).toLocaleDateString('es-AR'),
        s.clientName ?? '—', s.totalUsd, s.marginUsd
      ]);
    } else {
      return;
    }

    const csv = [headers, ...rows]
      .map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(','))
      .join('\n');

    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `ip-gestion-${tab}-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }
}
