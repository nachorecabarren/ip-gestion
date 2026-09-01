import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { Sale, CashMovement, SaleCategory } from '../../shared/models/models';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-base-datos',
  standalone: true,
  imports: [CommonModule, PaginationComponent],
  templateUrl: './base-datos.component.html',
  styleUrls: ['./base-datos.component.scss']
})
export class BaseDatosComponent implements OnInit {
  private api = inject(ApiService);

  activeTab = signal<'caja'|'minorista'|'mayorista'|'sv'|'reservas'>('caja');
  movements = signal<CashMovement[]>([]);
  sales = signal<Sale[]>([]);
  loading = signal(true);
  exporting = signal(false);

  page = signal(1);
  pageSize = signal(100);
  total = signal(0);

  // Cantidad/total de venta "completa" para las tarjetas de resumen — independiente
  // de qué página esté viendo en la tabla de abajo, por eso se pide aparte.
  summaryCount = signal(0);
  summaryTotalUsd = signal(0);

  ngOnInit() { this.load(); }

  setTab(tab: 'caja'|'minorista'|'mayorista'|'sv'|'reservas') {
    this.activeTab.set(tab);
    this.page.set(1);
    this.load();
  }

  onPageParams(p: PageParams) {
    this.page.set(p.page);
    this.pageSize.set(p.pageSize);
    this.load();
  }

  load() {
    this.loading.set(true);
    const tab = this.activeTab();
    if (tab === 'caja') {
      this.api.getCashMovements(undefined, undefined, undefined, this.page(), this.pageSize()).subscribe(r => {
        this.movements.set(r.items); this.total.set(r.total); this.loading.set(false);
      });
    } else if (tab === 'minorista' || tab === 'mayorista') {
      const category: SaleCategory = tab === 'minorista' ? 'RETAIL' : 'WHOLESALE';
      this.api.getSales(category, undefined, undefined, undefined, undefined, this.page(), this.pageSize()).subscribe(r => {
        this.sales.set(r.items); this.total.set(r.total); this.loading.set(false);
      });
      this.loadSummary(category);
    } else {
      this.loading.set(false);
    }
  }

  private loadSummary(category: SaleCategory) {
    this.api.getSales(category, undefined, undefined, undefined, undefined, 1, 1).subscribe(first => {
      if (first.total === 0) { this.summaryCount.set(0); this.summaryTotalUsd.set(0); return; }
      this.api.getSales(category, undefined, undefined, undefined, undefined, 1, first.total).subscribe(all => {
        const completed = all.items.filter(s => s.status === 'COMPLETED');
        this.summaryCount.set(completed.length);
        this.summaryTotalUsd.set(completed.reduce((s, x) => s + x.totalUsd, 0));
      });
    });
  }

  /** Ya vienen filtradas por categoría desde el servidor; acá solo se descartan las anuladas. */
  get retailSales() { return this.sales().filter(s => s.status === 'COMPLETED'); }
  get wholesaleSales() { return this.sales().filter(s => s.status === 'COMPLETED'); }

  private downloadCsv(name: string, headers: string[], rows: (string | number)[][]) {
    const csv = [headers, ...rows]
      .map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(','))
      .join('\n');
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `ip-gestion-${name}-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  /**
   * El export tiene que traer TODO, no lo que esté paginado en pantalla en ese
   * momento — por eso pide el total real primero y después una sola página con
   * ese tamaño, en vez de usar page()/pageSize() del estado de la tabla.
   */
  exportExcel() {
    const tab = this.activeTab();
    if (tab === 'sv' || tab === 'reservas') return;
    this.exporting.set(true);

    if (tab === 'caja') {
      this.api.getCashMovements(undefined, undefined, undefined, 1, 1).subscribe(first => {
        this.api.getCashMovements(undefined, undefined, undefined, 1, Math.max(first.total, 1)).subscribe(all => {
          this.downloadCsv('caja', ['Fecha', 'Tipo', 'Método', 'Caja', 'Detalle', 'Monto USD'],
            all.items.map(m => [
              new Date(m.createdAt).toLocaleDateString('es-AR'),
              m.type, m.method, m.cajaName, m.detail ?? '', m.amountUsd
            ]));
          this.exporting.set(false);
        });
      });
      return;
    }

    const category: SaleCategory = tab === 'minorista' ? 'RETAIL' : 'WHOLESALE';
    this.api.getSales(category, undefined, undefined, undefined, undefined, 1, 1).subscribe(first => {
      this.api.getSales(category, undefined, undefined, undefined, undefined, 1, Math.max(first.total, 1)).subscribe(all => {
        const rows = all.items
          .filter(s => s.status === 'COMPLETED')
          .map(s => [
            new Date(s.saleDate).toLocaleDateString('es-AR'),
            s.clientName ?? (tab === 'minorista' ? 'CF' : '—'), s.totalUsd, s.marginUsd
          ]);
        this.downloadCsv(tab, ['Fecha', 'Cliente', 'Total USD', 'Margen USD'], rows);
        this.exporting.set(false);
      });
    });
  }
}
