import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { AuthService } from '../../core/services/auth.service';
import { StockItem, StockBulk, CatalogLocation, CatalogModel, TradeInQuote, TradeInHistoryItem, Entity } from '../../shared/models/models';
import { ImeiScannerComponent } from '../../shared/components/imei-scanner/imei-scanner.component';
import { EscapeCloseDirective } from '../../shared/directives/escape-close.directive';
import { AutoFocusDirective } from '../../shared/directives/auto-focus.directive';
import { confirmDiscard } from '../../shared/utils/confirm-discard';

@Component({
  selector: 'app-stock',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImeiScannerComponent, EscapeCloseDirective, AutoFocusDirective],
  templateUrl: './stock.component.html',
  styleUrls: ['./stock.component.scss']
})
export class StockComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  auth = inject(AuthService);

  activeTab = signal<'equipos' | 'bulk' | 'canjes'>('equipos');
  items = signal<StockItem[]>([]);
  bulkItems = signal<StockBulk[]>([]);
  tradeInHistory = signal<TradeInHistoryItem[]>([]);
  total = signal(0);
  loading = signal(true);

  // Filters
  search = signal('');
  statusFilter = signal('');
  conditionFilter = signal('');
  colorFilter = signal('');
  storageFilter = signal('');

  readonly storageOptions = [64, 128, 256, 512, 1024, 2048];

  // New item modal
  showNewModal = signal(false);
  submitting = signal(false);
  newError = signal('');
  newForm!: FormGroup;
  models = signal<CatalogModel[]>([]);
  locations = signal<CatalogLocation[]>([]);
  providers = signal<Entity[]>([]);
  showAddModel = signal(false);
  newModelName = signal('');
  addingModel = signal(false);

  // Edit modal
  showEditModal = signal(false);
  editingItem = signal<StockItem | null>(null);
  editForm!: FormGroup;

  // Cotizador modal
  showCotizadorModal = signal(false);
  cotizadorForm!: FormGroup;
  cotizadorResult = signal<TradeInQuote | null>(null);
  cotizadorLoading = signal(false);

  scannerMessage = signal('');
  scannerMessageType = signal<'success' | 'error'>('success');

  ngOnInit() {
    this.loadItems();
    this.api.getCatalogLocations().subscribe(l => this.locations.set(l));
    this.api.getCatalogModels().subscribe(m => this.models.set(m));
    this.api.getEntities('PROVIDER').subscribe(r => this.providers.set(r.items));
    this.newForm = this.fb.group({
      providerId: [''],
      modelId: ['', Validators.required],
      imeiSerial: [''],
      color: [''],
      storageGb: [null],
      condition: ['USED', Validators.required],
      batteryPct: [null, [Validators.min(0), Validators.max(100)]],
      costUsd: [0, [Validators.required, Validators.min(0)]],
      suggestedPriceUsd: [null, [Validators.required, Validators.min(0.01)]],
      wholesalePriceUsd: [null],
      locationId: [''],
      notes: [''],
    });
    this.cotizadorForm = this.fb.group({
      modelName: ['', Validators.required],
      storageGb: [128, [Validators.required, Validators.min(1)]],
      batteryPct: [100, [Validators.required, Validators.min(0), Validators.max(100)]],
    });
    this.editForm = this.fb.group({
      color: [''],
      storageGb: [null],
      condition: ['USED'],
      conditionGrade: [''],
      batteryPct: [null, [Validators.min(0), Validators.max(100)]],
      suggestedPriceUsd: [0, [Validators.required, Validators.min(0)]],
      wholesalePriceUsd: [null],
      locationId: [null],
      notes: [''],
    });
  }

  hasActiveFilters(): boolean {
    return !!(this.search() || this.statusFilter() || this.conditionFilter() || this.colorFilter() || this.storageFilter());
  }

  clearFilters() {
    this.search.set('');
    this.statusFilter.set('');
    this.conditionFilter.set('');
    this.colorFilter.set('');
    this.storageFilter.set('');
    this.loadItems();
  }

  loadItems() {
    this.loading.set(true);
    this.api.getStockItems(
      this.statusFilter() as any || undefined,
      this.conditionFilter() as any || undefined,
      this.search() || undefined,
      1,
      20,
      this.colorFilter() || undefined,
      this.storageFilter() ? Number(this.storageFilter()) : undefined,
    ).subscribe({
      next: r => { this.items.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
    this.api.getStockBulk().subscribe(b => this.bulkItems.set(b));
    this.api.getTradeInHistory().subscribe(r => this.tradeInHistory.set(r.items));
  }

  openNew() {
    this.newForm.reset({ providerId: '', modelId: '', condition: 'USED', costUsd: 0, locationId: '', storageGb: '' });
    this.newError.set('');
    this.showAddModel.set(false);
    this.showNewModal.set(true);
  }

  openAddModel() {
    this.newModelName.set('');
    this.showAddModel.set(true);
  }

  cancelAddModel() {
    this.showAddModel.set(false);
  }

  addModel() {
    const name = this.newModelName().trim();
    if (!name) return;
    this.addingModel.set(true);
    this.api.createCatalogModel(name).subscribe({
      next: (model) => {
        this.models.update(list => [...list, model]);
        this.newForm.patchValue({ modelId: model.id });
        this.showAddModel.set(false);
        this.addingModel.set(false);
      },
      error: () => this.addingModel.set(false),
    });
  }

  async dismissNewModal() {
    if (!(await confirmDiscard(this.confirm, this.newForm))) return;
    this.showNewModal.set(false);
  }

  submitNew() {
    if (this.newForm.invalid) {
      this.newForm.markAllAsTouched();
      this.newError.set('Completá los campos obligatorios marcados en rojo.');
      return;
    }
    this.submitting.set(true);
    this.newError.set('');
    const v = this.newForm.value;
    // Se registra igual que "Nueva Compra > Equipos": una compra de un solo ítem,
    // así el equipo queda con trazabilidad de proveedor como cualquier otra compra.
    const dto = {
      providerId: v.providerId || null,
      purchaseDate: new Date().toISOString().split('T')[0],
      type: 'DEVICE',
      notes: v.notes || null,
      deviceItems: [{
        modelId: v.modelId,
        imeiSerial: v.imeiSerial || null,
        color: v.color || null,
        storageGb: v.storageGb ? Number(v.storageGb) : null,
        condition: v.condition,
        batteryPct: v.batteryPct ? Number(v.batteryPct) : null,
        costUsd: Number(v.costUsd) || 0,
        suggestedPriceUsd: Number(v.suggestedPriceUsd),
        wholesalePriceUsd: v.wholesalePriceUsd ? Number(v.wholesalePriceUsd) : null,
        locationId: v.locationId || null,
        notes: v.notes || null,
      }],
      bulkItems: [],
    };
    this.api.createPurchase(dto).subscribe({
      next: () => { this.showNewModal.set(false); this.loadItems(); this.submitting.set(false); },
      error: (e) => {
        const msg = e?.error?.error
          || e?.error?.title
          || (e?.status === 0 ? 'No se puede conectar con el servidor. ¿El backend está corriendo?' : null)
          || `Error ${e?.status}`;
        this.newError.set(msg);
        this.submitting.set(false);
      },
    });
  }

  openCotizador() {
    this.cotizadorForm.reset({ modelName: '', storageGb: 128, batteryPct: 100 });
    this.cotizadorResult.set(null);
    this.showCotizadorModal.set(true);
  }

  calcularCotizacion() {
    if (this.cotizadorForm.invalid) { this.cotizadorForm.markAllAsTouched(); return; }
    this.cotizadorLoading.set(true);
    const v = this.cotizadorForm.value;
    this.api.getTradeInQuote({ modelName: v.modelName, storageGb: Number(v.storageGb), batteryPct: Number(v.batteryPct) }).subscribe({
      next: r => { this.cotizadorResult.set(r); this.cotizadorLoading.set(false); },
      error: () => this.cotizadorLoading.set(false),
    });
  }

  openEdit(item: StockItem) {
    this.editingItem.set(item);
    const location = this.locations().find(l => l.name === item.locationName);
    this.editForm.patchValue({
      color: item.color, storageGb: item.storageGb, condition: item.condition,
      conditionGrade: item.conditionGrade, batteryPct: item.batteryPct,
      suggestedPriceUsd: item.suggestedPriceUsd, wholesalePriceUsd: item.wholesalePriceUsd,
      locationId: location?.id ?? null, notes: item.notes
    });
    this.editForm.markAsPristine();
    this.showEditModal.set(true);
  }

  async dismissEditModal() {
    if (!(await confirmDiscard(this.confirm, this.editForm))) return;
    this.showEditModal.set(false);
  }

  saveEdit() {
    if (this.editForm.invalid || !this.editingItem()) return;
    this.api.updateStockItem(this.editingItem()!.id, this.editForm.value).subscribe(() => {
      this.showEditModal.set(false);
      this.loadItems();
    });
  }

  async voidItem(id: string) {
    if (!await this.confirm.open('¿Anular este ítem del stock? El equipo quedará como no disponible.')) return;
    this.api.voidStockItem(id).subscribe(() => this.loadItems());
  }

  onImeiScanned(value: string) {
    this.scannerMessage.set('Buscando IMEI...');
    this.scannerMessageType.set('success');
    this.api.getStockItems(undefined, undefined, value).subscribe({
      next: r => {
        if (r.items.length === 1) {
          this.items.set(r.items);
          this.total.set(r.total);
          this.search.set(value);
          this.scannerMessage.set('Se encontró un equipo con ese IMEI.');
          this.scannerMessageType.set('success');
        } else {
          this.scannerMessage.set('No se encontró ningún equipo con ese IMEI.');
          this.scannerMessageType.set('error');
        }
      },
      error: () => {
        this.scannerMessage.set('No se pudo buscar por IMEI en este momento.');
        this.scannerMessageType.set('error');
      }
    });
  }

  onNewImeiScanned(value: string) {
    this.newForm.patchValue({ imeiSerial: value });
  }

  getConditionLabel(c: string) {
    return ({ NEW: 'Nuevo', USED: 'Usado', REFURBISHED: 'Reacondicionado', A_PLUS: 'A+', A: 'A', B: 'B' })[c] ?? c;
  }
  getConditionClass(c: string) {
    return ({ NEW: 'badge--green', USED: 'badge--amber', REFURBISHED: 'badge--blue', A_PLUS: 'badge--green', A: 'badge--blue', B: 'badge--gray' })[c] ?? 'badge--gray';
  }
  getStatusLabel(s: string) {
    return ({ AVAILABLE: 'Disponible', RESERVED: 'Reservado', SOLD: 'Vendido', IN_REPAIR: 'En reparación', VOIDED: 'Anulado' })[s] ?? s;
  }
  getStatusClass(s: string) {
    return ({ AVAILABLE: 'badge--green', RESERVED: 'badge--amber', SOLD: 'badge--gray', IN_REPAIR: 'badge--blue', VOIDED: 'badge--red' })[s] ?? 'badge--gray';
  }
}
