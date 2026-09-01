import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { CatalogModel, CatalogAccessory, CatalogLocation, AuditLog } from '../../shared/models/models';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PaginationComponent],
  template: `
  <div class="page-header"><div><h1 class="page-title">Configuración</h1><p class="page-sub">Catálogos, equipo de trabajo y ajustes del sistema</p></div></div>
  <div class="tabs">
    <button class="tab" [class.active]="tab()==='modelos'" (click)="tab.set('modelos')">Modelos</button>
    <button class="tab" [class.active]="tab()==='accesorios'" (click)="tab.set('accesorios')">Accesorios</button>
    <button class="tab" [class.active]="tab()==='ubicaciones'" (click)="tab.set('ubicaciones')">Ubicaciones</button>
    <button class="tab" [class.active]="tab()==='auditoria'" (click)="tab.set('auditoria'); loadAudit()">Auditoría</button>
  </div>

  <div *ngIf="error()" class="alert alert--error" style="margin-bottom:12px">{{ error() }}</div>

  <div class="card" *ngIf="tab()==='modelos'">
    <div class="catalog-header">
      <h3>Modelos de equipos</h3>
      <div class="form-row" [formGroup]="modelForm" style="gap:8px;margin:0">
        <input formControlName="name" class="form-control" placeholder="ej: iPhone 16 Pro" (keydown.enter)="addModel()" />
        <button class="btn btn--primary btn--sm" (click)="addModel()" [disabled]="saving()">
          {{ saving() ? 'Guardando…' : '+ Agregar' }}
        </button>
      </div>
    </div>
    <table class="table">
      <thead><tr><th>NOMBRE</th></tr></thead>
      <tbody>
        <tr *ngFor="let m of pagedModels()"><td>{{ m.name }}</td></tr>
        <tr *ngIf="models().length === 0"><td colspan="2" class="table__empty">Sin modelos cargados</td></tr>
      </tbody>
    </table>
    <app-pagination [page]="modelsPage()" [pageSize]="modelsPageSize()" [total]="models().length"
      (paramsChange)="modelsPage.set($event.page); modelsPageSize.set($event.pageSize)"></app-pagination>
  </div>

  <div class="card" *ngIf="tab()==='accesorios'">
    <div class="catalog-header">
      <h3>Accesorios</h3>
      <div class="form-row" [formGroup]="accForm" style="gap:8px;margin:0">
        <input formControlName="name" class="form-control" placeholder="ej: Funda transparente" (keydown.enter)="addAcc()" />
        <button class="btn btn--primary btn--sm" (click)="addAcc()" [disabled]="saving()">
          {{ saving() ? 'Guardando…' : '+ Agregar' }}
        </button>
      </div>
    </div>
    <table class="table">
      <thead><tr><th>NOMBRE</th></tr></thead>
      <tbody>
        <tr *ngFor="let a of pagedAccessories()"><td>{{ a.name }}</td></tr>
        <tr *ngIf="accessories().length === 0"><td class="table__empty">Sin accesorios cargados</td></tr>
      </tbody>
    </table>
    <app-pagination [page]="accPage()" [pageSize]="accPageSize()" [total]="accessories().length"
      (paramsChange)="accPage.set($event.page); accPageSize.set($event.pageSize)"></app-pagination>
  </div>

  <div class="card" *ngIf="tab()==='ubicaciones'">
    <div class="catalog-header">
      <h3>Ubicaciones de stock</h3>
      <div class="form-row" [formGroup]="locForm" style="gap:8px;margin:0">
        <input formControlName="name" class="form-control" placeholder="ej: Sucursal Palermo" (keydown.enter)="addLoc()" />
        <button class="btn btn--primary btn--sm" (click)="addLoc()" [disabled]="saving()">
          {{ saving() ? 'Guardando…' : '+ Agregar' }}
        </button>
      </div>
    </div>
    <table class="table">
      <thead><tr><th>UBICACIÓN</th></tr></thead>
      <tbody>
        <tr *ngFor="let l of pagedLocations()"><td>{{ l.name }}</td></tr>
        <tr *ngIf="locations().length === 0"><td class="table__empty">Sin ubicaciones cargadas</td></tr>
      </tbody>
    </table>
    <app-pagination [page]="locPage()" [pageSize]="locPageSize()" [total]="locations().length"
      (paramsChange)="locPage.set($event.page); locPageSize.set($event.pageSize)"></app-pagination>
  </div>

  <div class="card" *ngIf="tab()==='auditoria'">
    <div class="catalog-header">
      <h3>Auditoría</h3>
      <p class="page-sub" style="margin:0">Registro de acciones sensibles (ventas, compras, stock, caja, etc). Solo vos podés verlo.</p>
    </div>
    <table class="table">
      <thead><tr><th>FECHA</th><th>USUARIO</th><th>ACCIÓN</th><th>DETALLE</th></tr></thead>
      <tbody>
        <tr *ngFor="let log of auditLogs()">
          <td>{{ log.createdAt | date:'dd/MM/yy HH:mm' }}</td>
          <td>{{ log.userName }}</td>
          <td><span class="badge" [ngClass]="auditActionClass(log.action)">{{ auditActionLabel(log.action, log.entityType) }}</span></td>
          <td>{{ log.details ?? '—' }}</td>
        </tr>
        <tr *ngIf="!auditLoading() && auditLogs().length === 0"><td colspan="4" class="table__empty">Sin actividad registrada</td></tr>
        <tr *ngIf="auditLoading()"><td colspan="4" class="table__empty">Cargando…</td></tr>
      </tbody>
    </table>
    <app-pagination [page]="auditPage()" [pageSize]="auditPageSize()" [total]="auditTotal()"
      (paramsChange)="onAuditPageParams($event)"></app-pagination>
  </div>
  `,
  styleUrls: ['./configuracion.component.scss']
})
export class ConfiguracionComponent implements OnInit {
  private api = inject(ApiService); private fb = inject(FormBuilder);
  tab = signal<'modelos'|'accesorios'|'ubicaciones'|'auditoria'>('modelos');
  models = signal<CatalogModel[]>([]);
  accessories = signal<CatalogAccessory[]>([]);
  locations = signal<CatalogLocation[]>([]);
  saving = signal(false);
  error = signal('');
  modelForm!: FormGroup; accForm!: FormGroup; locForm!: FormGroup;

  // Los catálogos se traen completos (los usan otras pantallas como dropdown),
  // acá solo se pagina la vista.
  modelsPage = signal(1);
  modelsPageSize = signal(100);
  pagedModels = computed(() => {
    const start = (this.modelsPage() - 1) * this.modelsPageSize();
    return this.models().slice(start, start + this.modelsPageSize());
  });

  accPage = signal(1);
  accPageSize = signal(100);
  pagedAccessories = computed(() => {
    const start = (this.accPage() - 1) * this.accPageSize();
    return this.accessories().slice(start, start + this.accPageSize());
  });

  locPage = signal(1);
  locPageSize = signal(100);
  pagedLocations = computed(() => {
    const start = (this.locPage() - 1) * this.locPageSize();
    return this.locations().slice(start, start + this.locPageSize());
  });

  auditLogs = signal<AuditLog[]>([]);
  auditTotal = signal(0);
  auditPage = signal(1);
  auditPageSize = signal(100);
  auditLoading = signal(false);

  ngOnInit() {
    this.modelForm = this.fb.group({ name: [''] });
    this.accForm = this.fb.group({ name: [''] });
    this.locForm = this.fb.group({ name: [''] });
    this.api.getCatalogModels().subscribe(m => this.models.set(m));
    this.api.getCatalogAccessories().subscribe(a => this.accessories.set(a));
    this.api.getCatalogLocations().subscribe(l => this.locations.set(l));
  }

  addModel() {
    const n = this.modelForm.get('name')?.value?.trim();
    if (!n) return;
    this.saving.set(true); this.error.set('');
    this.api.createCatalogModel(n).subscribe({
      next: m => { this.models.update(arr => [...arr, m]); this.modelForm.reset(); this.saving.set(false); },
      error: e => { this.error.set(e?.error?.error ?? 'Error al agregar modelo'); this.saving.set(false); }
    });
  }
  addAcc() {
    const n = this.accForm.get('name')?.value?.trim();
    if (!n) return;
    this.saving.set(true); this.error.set('');
    this.api.createCatalogAccessory(n).subscribe({
      next: a => { this.accessories.update(arr => [...arr, a]); this.accForm.reset(); this.saving.set(false); },
      error: e => { this.error.set(e?.error?.error ?? 'Error al agregar accesorio'); this.saving.set(false); }
    });
  }
  addLoc() {
    const n = this.locForm.get('name')?.value?.trim();
    if (!n) return;
    this.saving.set(true); this.error.set('');
    this.api.createCatalogLocation(n).subscribe({
      next: l => { this.locations.update(arr => [...arr, l]); this.locForm.reset(); this.saving.set(false); },
      error: e => { this.error.set(e?.error?.error ?? 'Error al agregar ubicación'); this.saving.set(false); }
    });
  }

  loadAudit() {
    this.auditLoading.set(true);
    this.api.getAuditLog(this.auditPage(), this.auditPageSize()).subscribe({
      next: r => { this.auditLogs.set(r.items); this.auditTotal.set(r.total); this.auditLoading.set(false); },
      error: () => this.auditLoading.set(false),
    });
  }

  onAuditPageParams(p: PageParams) {
    this.auditPage.set(p.page);
    this.auditPageSize.set(p.pageSize);
    this.loadAudit();
  }

  private readonly auditActionLabels: Record<string, string> = {
    CREATE: 'Creación', VOID: 'Anulación', CANCEL: 'Cancelación', CONVERT: 'Conversión',
  };
  private readonly auditEntityLabels: Record<string, string> = {
    Sale: 'venta', Purchase: 'compra', StockItem: 'ítem de stock', Reservation: 'reserva',
    CashMovement: 'movimiento de caja', DebtPayment: 'pago/cobro',
  };

  auditActionLabel(action: string, entityType: string): string {
    const a = this.auditActionLabels[action] ?? action;
    const e = this.auditEntityLabels[entityType] ?? entityType;
    return `${a} de ${e}`;
  }

  auditActionClass(action: string): string {
    return ({ CREATE: 'badge--green', VOID: 'badge--red', CANCEL: 'badge--red', CONVERT: 'badge--blue' } as Record<string, string>)[action] ?? 'badge--gray';
  }
}
