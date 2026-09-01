import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PageParams {
  page: number;
  pageSize: number;
}

/**
 * Barra de paginación reusable: selector de tamaño de página + navegación.
 * Cambiar el tamaño de página siempre vuelve a la página 1 (los resultados
 * se corren), así que solo emite un evento combinado con ambos valores ya
 * resueltos para que el padre no tenga que sincronizarlo a mano.
 */
@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pagination" *ngIf="total > pageSize">
      <div class="pagination__size">
        <label>Mostrar</label>
        <select class="filter-select" [value]="pageSize" (change)="onPageSizeChange($any($event.target).value)">
          <option *ngFor="let s of pageSizeOptions" [value]="s">{{ s }}</option>
        </select>
      </div>
      <div class="pagination__nav">
        <button type="button" class="btn btn--ghost btn--sm" [disabled]="page <= 1" (click)="prev()">← Anterior</button>
        <span class="filter-count">Página {{ page }} de {{ totalPages }} · {{ total }} resultados</span>
        <button type="button" class="btn btn--ghost btn--sm" [disabled]="page >= totalPages" (click)="next()">Siguiente →</button>
      </div>
    </div>
  `,
  styles: [`
    .pagination {
      display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap;
      gap: 12px; padding: 12px 16px; border-top: 1px solid var(--color-border);
    }
    .pagination__size { display: flex; align-items: center; gap: 8px; }
    .pagination__size label { font-size: 12px; color: var(--color-text-muted); }
    .pagination__size select { padding: 5px 24px 5px 8px; font-size: 12px; }
    .pagination__nav { display: flex; align-items: center; gap: 10px; }
  `]
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() pageSize = 100;
  @Input() total = 0;
  @Input() pageSizeOptions = [50, 100, 500, 1000];
  @Output() paramsChange = new EventEmitter<PageParams>();

  get totalPages() {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  prev() {
    if (this.page > 1) this.paramsChange.emit({ page: this.page - 1, pageSize: this.pageSize });
  }

  next() {
    if (this.page < this.totalPages) this.paramsChange.emit({ page: this.page + 1, pageSize: this.pageSize });
  }

  onPageSizeChange(size: string) {
    this.paramsChange.emit({ page: 1, pageSize: Number(size) });
  }
}
