import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { CatalogModel, CatalogAccessory, CatalogLocation } from '../../shared/models/models';

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="page-header"><div><h1 class="page-title">Configuración</h1><p class="page-sub">Catálogos, equipo de trabajo y ajustes del sistema</p></div></div>
  <div class="tabs">
    <button class="tab" [class.active]="tab()==='modelos'" (click)="tab.set('modelos')">Modelos</button>
    <button class="tab" [class.active]="tab()==='accesorios'" (click)="tab.set('accesorios')">Accesorios</button>
    <button class="tab" [class.active]="tab()==='ubicaciones'" (click)="tab.set('ubicaciones')">Ubicaciones</button>
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
      <thead><tr><th>NOMBRE</th><th>ID TYPE</th></tr></thead>
      <tbody>
        <tr *ngFor="let m of models()"><td>{{ m.name }}</td><td>{{ m.idType }}</td></tr>
        <tr *ngIf="models().length === 0"><td colspan="2" class="table__empty">Sin modelos cargados</td></tr>
      </tbody>
    </table>
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
        <tr *ngFor="let a of accessories()"><td>{{ a.name }}</td></tr>
        <tr *ngIf="accessories().length === 0"><td class="table__empty">Sin accesorios cargados</td></tr>
      </tbody>
    </table>
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
        <tr *ngFor="let l of locations()"><td>{{ l.name }}</td></tr>
        <tr *ngIf="locations().length === 0"><td class="table__empty">Sin ubicaciones cargadas</td></tr>
      </tbody>
    </table>
  </div>
  `,
  styleUrls: ['./configuracion.component.scss']
})
export class ConfiguracionComponent implements OnInit {
  private api = inject(ApiService); private fb = inject(FormBuilder);
  tab = signal<'modelos'|'accesorios'|'ubicaciones'>('modelos');
  models = signal<CatalogModel[]>([]);
  accessories = signal<CatalogAccessory[]>([]);
  locations = signal<CatalogLocation[]>([]);
  saving = signal(false);
  error = signal('');
  modelForm!: FormGroup; accForm!: FormGroup; locForm!: FormGroup;

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
}
