import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { AuthService } from '../../core/services/auth.service';
import { Reservation, Entity, StockItem, PaymentMethod } from '../../shared/models/models';
import { EscapeCloseDirective } from '../../shared/directives/escape-close.directive';
import { AutoFocusDirective } from '../../shared/directives/auto-focus.directive';
import { openIfQueryParam } from '../../shared/utils/open-via-query-param';
import { confirmDiscard } from '../../shared/utils/confirm-discard';
import { ImeiScannerComponent } from '../../shared/components/imei-scanner/imei-scanner.component';
import { StockItemSelectComponent } from '../../shared/components/stock-item-select/stock-item-select.component';
import { PaginationComponent, PageParams } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-reservas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, EscapeCloseDirective, AutoFocusDirective, ImeiScannerComponent, StockItemSelectComponent, PaginationComponent],
  templateUrl: './reservas.component.html',
  styleUrls: ['./reservas.component.scss']
})
export class ReservasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  auth = inject(AuthService);

  reservations = signal<Reservation[]>([]);
  total = signal(0);
  page = signal(1);
  pageSize = signal(100);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  statusFilter = signal('ACTIVE');

  clients = signal<Entity[]>([]);
  availableStock = signal<StockItem[]>([]);
  tcBlue = signal(1520);
  imeiScanMessage = signal('');
  imeiScanMessageType = signal<'success' | 'error'>('success');

  form!: FormGroup;

  // Convertir a venta
  showConvertModal = signal(false);
  convertingReservation = signal<Reservation | null>(null);
  convertForm!: FormGroup;
  convertSubmitting = signal(false);
  convertError = signal('');

  readonly paymentMethods: { value: PaymentMethod; label: string }[] = [
    { value: 'USD_CASH', label: 'Efectivo USD' },
    { value: 'USDT', label: 'USDT' },
    { value: 'ARS_CASH', label: 'Efectivo ARS' },
    { value: 'ARS_TR', label: 'Transferencia ARS' },
    { value: 'MERCADOPAGO', label: 'MercadoPago' },
  ];

  ngOnInit() {
    this.initForm();
    this.load();
    this.api.getEntities('CLIENT').subscribe(r => this.clients.set(r.items));
    this.api.getStockItems('AVAILABLE', undefined, undefined, 1, 500).subscribe(r => this.availableStock.set(r.items));
    this.api.getTcBlue().subscribe(r => this.tcBlue.set(r.rate));
    openIfQueryParam(this.route, this.router, 'nueva', () => this.openModal());
  }

  openModal() {
    this.initForm();
    this.showModal.set(true);
  }

  async dismissModal() {
    if (!(await confirmDiscard(this.confirm, this.form))) return;
    this.showModal.set(false);
  }

  onImeiScanned(value: string) {
    const match = this.availableStock().find(s => s.imeiSerial === value);
    if (match) {
      this.form.patchValue({ stockItemId: match.id });
      this.imeiScanMessage.set(`Equipo encontrado: ${match.modelName}`);
      this.imeiScanMessageType.set('success');
    } else {
      this.imeiScanMessage.set('No se encontró un equipo disponible con ese IMEI.');
      this.imeiScanMessageType.set('error');
    }
  }

  /** IMEI del equipo seleccionado en el dropdown, venga de un escaneo o de una selección manual. */
  selectedImei(): string | null {
    const id = this.form?.get('stockItemId')?.value;
    if (!id) return null;
    return this.availableStock().find(s => s.id === id)?.imeiSerial ?? null;
  }

  initForm() {
    this.imeiScanMessage.set('');
    this.form = this.fb.group({
      isConsumerFinal: [true],
      entityId: [null],
      retailClientName: [''],
      retailClientPhone: [''],
      retailClientInstagram: [''],
      stockItemId: [null],
      saleCategory: ['RETAIL'],
      pickupDate: ['', Validators.required],
      agreedPriceUsd: [0, [Validators.required, Validators.min(0.01)]],
      depositAmountUsd: [0, [Validators.required, Validators.min(0)]],
      depositMethod: ['USD_CASH'],
      notes: [''],
    });
    // Manually picking from the dropdown clears a stale scan message. A successful
    // scan re-sets it right after, since patchValue emits synchronously in onImeiScanned.
    this.form.get('stockItemId')!.valueChanges.subscribe(() => this.imeiScanMessage.set(''));
  }

  load() {
    this.loading.set(true);
    this.api.getReservations(this.statusFilter() as any || undefined, this.page(), this.pageSize()).subscribe({
      next: r => { this.reservations.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
  }

  applyFilters() {
    this.page.set(1);
    this.load();
  }

  onPageParams(p: PageParams) {
    this.page.set(p.page);
    this.pageSize.set(p.pageSize);
    this.load();
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    const v = this.form.value;
    const dto = {
      entityId: v.isConsumerFinal ? null : v.entityId,
      retailClientName: v.isConsumerFinal ? v.retailClientName : null,
      retailClientPhone: v.isConsumerFinal ? v.retailClientPhone : null,
      retailClientInstagram: v.isConsumerFinal ? v.retailClientInstagram : null,
      stockItemId: v.stockItemId || null,
      saleCategory: v.saleCategory,
      pickupDate: v.pickupDate,
      agreedPriceUsd: v.agreedPriceUsd,
      depositAmountUsd: v.depositAmountUsd,
      depositMethod: v.depositMethod,
      notes: v.notes,
    };
    this.api.createReservation(dto).subscribe({
      next: () => { this.showModal.set(false); this.load(); this.submitting.set(false); },
      error: () => this.submitting.set(false)
    });
  }

  async cancel(id: string) {
    if (!await this.confirm.open('¿Cancelar esta reserva? El cliente perderá el apartado.')) return;
    this.api.cancelReservation(id).subscribe(() => this.load());
  }

  get convertPayments() {
    return this.convertForm.get('payments') as FormArray;
  }

  addConvertPayment() {
    this.convertPayments.push(
      this.fb.group({
        method: ['USD_CASH', Validators.required],
        currency: ['USD'],
        amount: [0, [Validators.required, Validators.min(0.01)]],
        exchangeRateUsd: [1],
      }),
    );
  }

  removeConvertPayment(i: number) {
    this.convertPayments.removeAt(i);
  }

  onConvertMethodChange(i: number, event: Event) {
    const method = (event.target as HTMLSelectElement).value;
    const isArs = method === 'ARS_CASH' || method === 'ARS_TR' || method === 'MERCADOPAGO';
    this.convertPayments.at(i).patchValue({
      currency: isArs ? 'ARS' : 'USD',
      exchangeRateUsd: isArs ? this.tcBlue() : 1,
    });
  }

  openConvert(r: Reservation) {
    this.convertingReservation.set(r);
    this.convertError.set('');
    this.convertForm = this.fb.group({
      warrantyDays: [90, [Validators.required, Validators.min(0)]],
      payments: this.fb.array([]),
    });
    this.addConvertPayment();
    this.convertPayments.at(0).patchValue({ amount: r.agreedPriceUsd });
    this.showConvertModal.set(true);
  }

  async dismissConvertModal() {
    if (!(await confirmDiscard(this.confirm, this.convertForm))) return;
    this.showConvertModal.set(false);
  }

  get convertPaymentsTotal() {
    return this.convertPayments.controls.reduce((sum, c) => sum + (c.get('amount')?.value || 0), 0);
  }

  submitConvert() {
    const r = this.convertingReservation();
    if (!r || this.convertForm.invalid) { this.convertForm.markAllAsTouched(); return; }
    this.convertSubmitting.set(true);
    this.convertError.set('');
    const v = this.convertForm.value;
    const dto = {
      saleDate: new Date().toISOString().split('T')[0],
      entityId: r.entityId || null,
      retailClientName: r.entityId ? null : r.clientName,
      retailClientPhone: r.entityId ? null : r.clientPhone,
      saleCategory: r.category,
      origin: 'RESERVATION',
      totalUsd: r.agreedPriceUsd,
      warrantyDays: v.warrantyDays,
      notes: null,
      items: [{ type: 'EQUIPMENT', stockItemId: r.stockItemId, stockBulkId: null, quantity: 1, priceUsd: r.agreedPriceUsd }],
      payments: v.payments,
      closerIds: [],
      tradeIn: null,
    };
    this.api.convertReservationToSale(r.id, dto).subscribe({
      next: () => { this.showConvertModal.set(false); this.convertSubmitting.set(false); this.load(); },
      error: (e) => {
        this.convertError.set(e?.error?.error || e?.error?.title || `Error ${e?.status}`);
        this.convertSubmitting.set(false);
      },
    });
  }

  getStatusClass(s: string) {
    return ({ ACTIVE: 'badge--amber', SOLD: 'badge--green', CANCELLED: 'badge--red' })[s] ?? 'badge--gray';
  }
  getStatusLabel(s: string) {
    return ({ ACTIVE: 'Activa', SOLD: 'Concretada', CANCELLED: 'Cancelada' })[s] ?? s;
  }

  isPastPickup(d: string) { return new Date(d) < new Date(); }

  formatUsd(v: number) {
    return `u$d ${v.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
