import { Component, OnInit, inject, signal } from '@angular/core';
import { ConfirmService } from '../../shared/services/confirm.service';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Purchase, Entity, CatalogModel, CatalogAccessory, CatalogLocation } from '../../shared/models/models';
import { ImeiScannerComponent } from '../../shared/components/imei-scanner/imei-scanner.component';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';
import { EscapeCloseDirective } from '../../shared/directives/escape-close.directive';
import { AutoFocusDirective } from '../../shared/directives/auto-focus.directive';
import { openIfQueryParam } from '../../shared/utils/open-via-query-param';
import { confirmDiscard } from '../../shared/utils/confirm-discard';

@Component({
  selector: 'app-compras',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImeiScannerComponent, PaginationComponent, EscapeCloseDirective, AutoFocusDirective],
  templateUrl: './compras.component.html',
  styleUrls: ['./compras.component.scss']
})
export class ComprasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  auth = inject(AuthService);

  purchases = signal<Purchase[]>([]);
  total = signal(0);
  page = signal(1);
  pageSize = signal(100);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  error = signal('');
  activeTab = signal<'device' | 'bulk'>('device');

  providers = signal<Entity[]>([]);
  models = signal<CatalogModel[]>([]);
  accessories = signal<CatalogAccessory[]>([]);
  locations = signal<CatalogLocation[]>([]);

  addAccessoryOpenIndex = signal<number | null>(null);
  newAccessoryName = signal('');
  addingAccessory = signal(false);

  form!: FormGroup;

  ngOnInit() {
    this.initForm();
    this.loadPurchases();
    this.api.getEntities('PROVIDER').subscribe(r => this.providers.set(r.items));
    this.api.getCatalogModels().subscribe(m => this.models.set(m));
    this.api.getCatalogAccessories().subscribe(a => this.accessories.set(a));
    this.api.getCatalogLocations().subscribe(l => this.locations.set(l));
    openIfQueryParam(this.route, this.router, 'nueva', () => this.openModal());
  }

  initForm() {
    this.form = this.fb.group({
      providerId: [null],
      purchaseDate: [new Date().toISOString().split('T')[0], Validators.required],
      type: ['DEVICE'],
      notes: [''],
      deviceItems: this.fb.array([]),
      bulkItems: this.fb.array([]),
    });
  }

  get deviceItems() { return this.form.get('deviceItems') as FormArray; }
  get bulkItems() { return this.form.get('bulkItems') as FormArray; }

  addDevice() {
    this.deviceItems.push(this.fb.group({
      modelId: [null, Validators.required],
      imeiSerial: [''],
      color: [''],
      storageGb: [null],
      condition: ['NEW', Validators.required],
      batteryPct: [null],
      costUsd: [0, [Validators.required, Validators.min(0.01)]],
      suggestedPriceUsd: [0, [Validators.required, Validators.min(0.01)]],
      wholesalePriceUsd: [null],
      locationId: [null],
      notes: [''],
    }));
  }

  removeDevice(i: number) { this.deviceItems.removeAt(i); }

  addBulk() {
    this.bulkItems.push(this.fb.group({
      accessoryId: [null, Validators.required],
      modelId: [null],
      color: [''],
      quantity: [1, [Validators.required, Validators.min(1)]],
      costUsd: [0, [Validators.required, Validators.min(0.01)]],
      suggestedPriceUsd: [0, [Validators.required, Validators.min(0.01)]],
    }));
  }

  removeBulk(i: number) {
    this.bulkItems.removeAt(i);
    this.addAccessoryOpenIndex.set(null);
  }

  openAddAccessory(i: number) {
    this.newAccessoryName.set('');
    this.addAccessoryOpenIndex.set(i);
  }

  cancelAddAccessory() {
    this.addAccessoryOpenIndex.set(null);
  }

  addAccessory(i: number) {
    const name = this.newAccessoryName().trim();
    if (!name) return;
    this.addingAccessory.set(true);
    this.api.createCatalogAccessory(name).subscribe({
      next: (accessory) => {
        this.accessories.update(list => [...list, accessory]);
        this.bulkItems.at(i).patchValue({ accessoryId: accessory.id });
        this.addAccessoryOpenIndex.set(null);
        this.addingAccessory.set(false);
      },
      error: () => this.addingAccessory.set(false),
    });
  }

  loadPurchases() {
    this.loading.set(true);
    this.api.getPurchases(this.page(), this.pageSize()).subscribe({
      next: r => { this.purchases.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
  }

  onPageParams(p: PageParams) {
    this.page.set(p.page);
    this.pageSize.set(p.pageSize);
    this.loadPurchases();
  }

  openModal() {
    this.initForm();
    this.activeTab.set('device');
    this.addAccessoryOpenIndex.set(null);
    this.error.set('');
    this.showModal.set(true);
  }

  async dismissModal() {
    if (!(await confirmDiscard(this.confirm, this.form))) return;
    this.showModal.set(false);
  }

  submit() {
    this.error.set('');

    if (this.deviceItems.length === 0 && this.bulkItems.length === 0) {
      this.error.set('Agregá al menos un equipo o un accesorio antes de guardar.');
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Completá los campos obligatorios marcados en rojo.');
      return;
    }

    this.submitting.set(true);
    const v = this.form.value;
    const dto = {
      providerId: v.providerId || null,
      purchaseDate: v.purchaseDate,
      type: v.type,
      notes: v.notes,
      deviceItems: v.deviceItems,
      bulkItems: v.bulkItems,
    };
    this.api.createPurchase(dto).subscribe({
      next: () => { this.showModal.set(false); this.loadPurchases(); this.submitting.set(false); },
      error: (e) => {
        this.submitting.set(false);
        this.error.set(e?.error?.error || e?.error?.title || `Error ${e?.status}`);
      }
    });
  }

  async voidPurchase(id: string) {
    if (!await this.confirm.open('¿Anular esta compra? Esta acción no se puede deshacer.')) return;
    this.api.voidPurchase(id).subscribe(() => this.loadPurchases());
  }

  getStatusClass(s: string) {
    return ({ ACTIVE: 'badge--green', PENDING: 'badge--amber', CANCELLED: 'badge--red', DELIVERED: 'badge--blue' } as Record<string,string>)[s] ?? 'badge--gray';
  }
  getStatusLabel(s: string) {
    return ({ ACTIVE: 'Activa', PENDING: 'Pendiente', CANCELLED: 'Anulada', DELIVERED: 'Entregada' } as Record<string,string>)[s] ?? s;
  }

  onImeiScanned(index: number, value: string) {
    this.deviceItems.at(index).patchValue({ imeiSerial: value });
  }

  get totalCosto() {
    const d = this.deviceItems.controls.reduce((s, c) => s + (c.get('costUsd')?.value || 0), 0);
    const b = this.bulkItems.controls.reduce((s, c) => s + (c.get('costUsd')?.value || 0) * (c.get('quantity')?.value || 1), 0);
    return d + b;
  }
}
