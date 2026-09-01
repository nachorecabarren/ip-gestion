// ─── PROVEEDORES ──────────────────────────────────────────
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Entity } from '../../shared/models/models';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';

// ─── PROVEEDORES ──────────────────────────────────────────
@Component({ selector:'app-proveedores', standalone:true, imports:[CommonModule,ReactiveFormsModule,PaginationComponent],
  styleUrls:['./proveedores.component.scss'],
  template:`
  <div class="page-header"><div><h1 class="page-title">Proveedores</h1><p class="page-sub">Catálogo de proveedores y condiciones</p></div>
    <button class="btn btn--primary" (click)="showModal.set(true)">+ Nuevo Proveedor</button></div>
  <div class="card">
    <div *ngIf="loading()" class="empty-state">Cargando...</div>
    <table class="table" *ngIf="!loading()">
      <thead><tr><th>NOMBRE</th><th>TELÉFONO</th><th>EMAIL</th><th>CIUDAD</th><th>BALANCE</th></tr></thead>
      <tbody>
        <tr *ngFor="let p of providers()">
          <td><strong>{{ p.name }}</strong></td><td>{{ p.phone ?? '—' }}</td>
          <td>{{ p.email ?? '—' }}</td><td>{{ p.addressCity ?? '—' }}</td>
          <td [class.text-red]="(p.balanceUsd ?? 0) < 0">u$d {{ p.balanceUsd?.toFixed(2) ?? '0.00' }}</td>
        </tr>
      </tbody>
    </table>
    <app-pagination
      [page]="page()"
      [pageSize]="pageSize()"
      [total]="total()"
      (paramsChange)="onPageParams($event)"
    ></app-pagination>
  </div>
  <div class="modal-overlay" *ngIf="showModal()" (click)="showModal.set(false)">
    <div class="modal" (click)="$event.stopPropagation()">
      <div class="modal__header"><div><h2 class="modal__title">Nuevo Proveedor</h2></div>
        <button class="modal__close" (click)="showModal.set(false)">✕</button></div>
      <div class="modal__body" [formGroup]="form">
        <div class="form-group"><label>Nombre *</label><input formControlName="name" class="form-control" /></div>
        <div class="form-row">
          <div class="form-group form-group--grow"><label>Teléfono</label><input formControlName="phone" class="form-control" /></div>
          <div class="form-group form-group--grow"><label>Email</label><input formControlName="email" class="form-control" /></div>
        </div>
        <div class="form-group"><label>Ciudad</label><input formControlName="addressCity" class="form-control" /></div>
      </div>
      <div class="modal__footer">
        <button class="btn btn--ghost" (click)="showModal.set(false)">Cancelar</button>
        <button class="btn btn--primary" (click)="submit()">Guardar</button>
      </div>
    </div>
  </div>
  `
})
export class ProveedoresComponent implements OnInit {
  private api = inject(ApiService); private fb = inject(FormBuilder);
  providers = signal<Entity[]>([]); loading = signal(true); showModal = signal(false);
  total = signal(0); page = signal(1); pageSize = signal(100);
  form!: FormGroup;
  ngOnInit() {
    this.form = this.fb.group({ name:['',Validators.required], phone:[''], email:[''], addressCity:[''], type:['PROVIDER'] });
    this.load();
  }
  load() {
    this.loading.set(true);
    this.api.getEntities('PROVIDER', undefined, this.page(), this.pageSize()).subscribe(r => {
      this.providers.set(r.items); this.total.set(r.total); this.loading.set(false);
    });
  }
  onPageParams(p: PageParams) {
    this.page.set(p.page);
    this.pageSize.set(p.pageSize);
    this.load();
  }
  submit() {
    if (this.form.invalid) return;
    this.api.createEntity(this.form.value).subscribe(() => { this.showModal.set(false); this.load(); });
  }
}
