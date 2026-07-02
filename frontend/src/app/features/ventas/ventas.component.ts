import { Component, OnInit, inject, signal, computed } from "@angular/core";
import { ConfirmService } from "../../shared/services/confirm.service";
import { AuthService } from "../../core/services/auth.service";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import {
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  ReactiveFormsModule,
} from "@angular/forms";
import { ApiService } from "../../core/services/api.service";
import {
  Sale,
  StockItem,
  Entity,
  CatalogModel,
  PaymentMethod,
  SaleCategory,
} from "../../shared/models/models";
import { ImeiScannerComponent } from "../../shared/components/imei-scanner/imei-scanner.component";

@Component({
  selector: "app-ventas",
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, ImeiScannerComponent],
  templateUrl: "./ventas.component.html",
  styleUrls: ["./ventas.component.scss"],
})
export class VentasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  auth = inject(AuthService);

  sales = signal<Sale[]>([]);
  total = signal(0);
  loading = signal(true);
  showModal = signal(false);
  wizardStep = signal(1);
  submitting = signal(false);
  submitError = signal('');

  // Catalog data
  entities = signal<Entity[]>([]);
  availableStock = signal<StockItem[]>([]);
  tcBlue = signal(1520);

  // Search/filter
  search = signal("");
  filterOrigin = signal("");
  filterStatus = signal("");

  // Summary panel
  totalItems = signal(0);
  totalPago = signal(0);

  // IMEI search per item row
  itemFilters = signal<string[]>([]);

  dateFrom = signal(new Date().toISOString().split("T")[0]); // hoy por defecto
  dateTo = signal(new Date().toISOString().split("T")[0]);
  filterByDate = signal(false);

  filteredSales = computed(() => {
    const status = this.filterStatus();
    return status
      ? this.sales().filter((s) => s.status === status)
      : this.sales();
  });

  saleForm!: FormGroup;

  readonly paymentMethods: { value: PaymentMethod; label: string }[] = [
    { value: "USD_CASH", label: "Efectivo USD" },
    { value: "USDT", label: "USDT" },
    { value: "ARS_CASH", label: "Efectivo ARS" },
    { value: "ARS_TR", label: "Transferencia ARS" },
    { value: "MERCADOPAGO", label: "MercadoPago" },
  ];

  ngOnInit() {
    this.initForm();
    this.loadSales();
    this.api.getTcBlue().subscribe((r) => this.tcBlue.set(r.rate));
    this.api.getEntities("CLIENT").subscribe((r) => this.entities.set(r.items));
    this.api
      .getStockItems("AVAILABLE")
      .subscribe((r) => this.availableStock.set(r.items));
  }

  initForm() {
    this.saleForm = this.fb.group({
      saleDate: [new Date().toISOString().split("T")[0], Validators.required],
      saleCategory: ["RETAIL"],
      entityId: [null],
      retailClientName: [""],
      retailClientPhone: [""],
      retailClientInstagram: [""],
      isConsumerFinal: [true],
      warrantyDays: [90, [Validators.required, Validators.min(0)]],
      notes: [""],
      items: this.fb.array([]),
      payments: this.fb.array([]),
      tradeInEnabled: [false],
      tradeInModel: [""],
      tradeInStorage: [null],
      tradeInBattery: [null],
      tradeInValue: [0],
    });
  }

  get items() {
    return this.saleForm.get("items") as FormArray;
  }
  get payments() {
    return this.saleForm.get("payments") as FormArray;
  }

  loadSales() {
    this.loading.set(true);
    this.api
      .getSales(
        undefined,
        undefined,
        this.search() || undefined,
        this.filterByDate() ? this.dateFrom() : undefined,
        this.filterByDate() ? this.dateTo() : undefined,
      )
      .subscribe({
        next: (r) => {
          this.sales.set(r.items);
          this.total.set(r.total);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  get completedSales() { return this.filteredSales().filter(s => s.status === 'COMPLETED'); }
  get ventasHoy() { return this.sales().filter(s => s.status === 'COMPLETED' && s.saleDate.startsWith(new Date().toISOString().split('T')[0])); }
  get totalHoy() { return this.ventasHoy.reduce((sum, s) => sum + s.totalUsd, 0); }
  get totalVentas() { return this.completedSales.reduce((sum, s) => sum + s.totalUsd, 0); }
  get retailCount() { return this.completedSales.filter(s => s.category === 'RETAIL').length; }
  get wholesaleCount() { return this.completedSales.filter(s => s.category === 'WHOLESALE').length; }
  get ticketPromedio() { return this.completedSales.length > 0 ? this.totalVentas / this.completedSales.length : 0; }
  get margenBruto() { return this.completedSales.reduce((sum, s) => sum + s.marginUsd, 0); }

  openNewSale() {
    this.initForm();
    this.itemFilters.set([]);
    this.wizardStep.set(1);
    this.submitError.set('');
    this.showModal.set(true);
    this.addItem();
    this.addPayment();
  }

  closeModal() {
    this.submitError.set('');
    this.showModal.set(false);
  }

  nextStep() {
    if (this.wizardStep() < 4) this.wizardStep.update((s) => s + 1);
  }
  prevStep() {
    if (this.wizardStep() > 1) this.wizardStep.update((s) => s - 1);
  }

  filteredStockFor(i: number): StockItem[] {
    const q = (this.itemFilters()[i] ?? '').toLowerCase().trim();
    if (!q) return this.availableStock();
    return this.availableStock().filter(s =>
      s.modelName.toLowerCase().includes(q) ||
      (s.imeiSerial ?? '').toLowerCase().includes(q) ||
      (s.internalCode ?? '').toLowerCase().includes(q) ||
      (s.color ?? '').toLowerCase().includes(q)
    );
  }

  setItemFilter(i: number, val: string) {
    const arr = [...this.itemFilters()];
    while (arr.length <= i) arr.push('');
    arr[i] = val;
    this.itemFilters.set(arr);
  }

  onImeiScanned(index: number, value: string) {
    this.setItemFilter(index, value);
    const matches = this.filteredStockFor(index);
    if (matches.length === 1) {
      const match = matches[0];
      this.items.at(index).patchValue({ stockItemId: match.id });
      const isWholesale = this.saleForm.get("saleCategory")?.value === "WHOLESALE";
      const price = isWholesale && match.wholesalePriceUsd
        ? match.wholesalePriceUsd
        : match.suggestedPriceUsd;
      this.items.at(index).patchValue({ priceUsd: price });
      this.updateTotals();
    }
  }

  addItem() {
    this.itemFilters.update(arr => [...arr, '']);
    this.items.push(
      this.fb.group({
        type: ["EQUIPMENT"],
        stockItemId: [null, Validators.required],
        stockBulkId: [null],
        quantity: [1, [Validators.min(1)]],
        priceUsd: [0, [Validators.required, Validators.min(0.01)]],
      }),
    );
  }

  removeItem(i: number) {
    this.items.removeAt(i);
    this.itemFilters.update(arr => arr.filter((_, idx) => idx !== i));
  }

  addPayment() {
    this.payments.push(
      this.fb.group({
        method: ["USD_CASH", Validators.required],
        currency: ["USD"],
        amount: [0, [Validators.required, Validators.min(0.01)]],
        exchangeRateUsd: [1],
      }),
    );
  }

  removePayment(i: number) {
    this.payments.removeAt(i);
  }

  onStockSelect(index: number, event: Event) {
    const id = (event.target as HTMLSelectElement).value;
    const item = this.availableStock().find((s) => s.id === id);
    if (item) {
      const isWholesale =
        this.saleForm.get("saleCategory")?.value === "WHOLESALE";
      const price =
        isWholesale && item.wholesalePriceUsd
          ? item.wholesalePriceUsd
          : item.suggestedPriceUsd;
      this.items.at(index).patchValue({ priceUsd: price });
      this.updateTotals();
    }
  }

  onMethodChange(index: number, event: Event) {
    const method = (event.target as HTMLSelectElement).value;
    const isArs =
      method === "ARS_CASH" || method === "ARS_TR" || method === "MERCADOPAGO";
    this.payments.at(index).patchValue({
      currency: isArs ? "ARS" : "USD",
      exchangeRateUsd: isArs ? this.tcBlue() : 1,
    });
  }

  updateTotals() {
    const total = this.items.controls.reduce(
      (sum, ctrl) =>
        sum +
        (ctrl.get("priceUsd")?.value || 0) * (ctrl.get("quantity")?.value || 1),
      0,
    );
    this.totalItems.set(total);
    const tradeIn = this.saleForm.get("tradeInEnabled")?.value
      ? this.saleForm.get("tradeInValue")?.value || 0
      : 0;
    this.totalPago.set(total - tradeIn);
  }

  submitSale() {
    const raw = this.saleForm.getRawValue();
    const hasItems = (raw.items || []).some((item: any) => item?.stockItemId || item?.stockBulkId);
    const total = Math.max(this.totalPago(), 0);

    if (!hasItems) {
      this.submitError.set('Seleccioná al menos un equipo para la venta.');
      this.saleForm.markAllAsTouched();
      return;
    }

    const normalizedPayments = (raw.payments || []).map((p: any) => ({
      method: p?.method || 'USD_CASH',
      currency: p?.currency || 'USD',
      amount: Number(p?.amount) > 0 ? Number(p.amount) : total,
      exchangeRateUsd: Number(p?.exchangeRateUsd) > 0 ? Number(p.exchangeRateUsd) : 1,
    }));

    if (normalizedPayments.length === 0) {
      normalizedPayments.push({ method: 'USD_CASH', currency: 'USD', amount: total, exchangeRateUsd: 1 });
    }

    this.payments.controls.forEach((control, index) => {
      const payment = normalizedPayments[index] || normalizedPayments[0];
      control.patchValue({
        method: payment.method,
        currency: payment.currency,
        amount: payment.amount,
        exchangeRateUsd: payment.exchangeRateUsd,
      });
    });

    this.saleForm.updateValueAndValidity();

    if (this.saleForm.invalid) {
      this.saleForm.markAllAsTouched();
      this.submitError.set('Completá los datos obligatorios antes de finalizar la venta.');
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');

    const saleDateUtc = raw.saleDate
      ? new Date(`${raw.saleDate}T00:00:00Z`).toISOString()
      : new Date().toISOString();

    const dto = {
      saleDate: saleDateUtc,
      entityId: raw.isConsumerFinal ? null : raw.entityId || null,
      retailClientName: raw.isConsumerFinal ? raw.retailClientName || null : null,
      retailClientPhone: raw.isConsumerFinal ? raw.retailClientPhone || null : null,
      retailClientInstagram: raw.isConsumerFinal ? raw.retailClientInstagram || null : null,
      saleCategory: raw.saleCategory,
      origin: 'DIRECT',
      totalUsd: total,
      warrantyDays: raw.warrantyDays,
      notes: raw.notes || null,
      items: (raw.items || []).map((item: any) => ({
        type: item.type,
        stockItemId: item.stockItemId || null,
        stockBulkId: item.stockBulkId || null,
        quantity: Number(item.quantity) || 1,
        priceUsd: Number(item.priceUsd) || 0,
      })),
      payments: normalizedPayments,
      closerIds: [],
      tradeIn: raw.tradeInEnabled
        ? {
            modelName: raw.tradeInModel || '',
            storageGb: raw.tradeInStorage,
            batteryPct: raw.tradeInBattery,
            valueUsd: raw.tradeInValue || 0,
          }
        : null,
    };
    this.api.createSale(dto).subscribe({
      next: () => {
        this.closeModal();
        this.loadSales();
        this.submitting.set(false);
      },
      error: (e) => {
        this.submitting.set(false);
        const message = e?.error?.error || e?.error?.title || (e?.status === 0 ? 'No se puede conectar con el servidor.' : 'No se pudo finalizar la venta.');
        this.submitError.set(message);
      },
    });
  }

  async voidSale(id: string) {
    if (!await this.confirm.open('¿Anular esta venta? Esta acción no se puede deshacer.')) return;
    this.api.voidSale(id).subscribe(() => this.loadSales());
  }

  formatUsd(v: number) {
    return `u$d ${v.toLocaleString("es-AR", { minimumFractionDigits: 2 })}`;
  }
  getStatusLabel(s: string) {
    return (
      {
        COMPLETED: "Completada",
        VOIDED: "Anulada",
        CANCELLED: "Cancelada",
        PENDING: "Pendiente",
      }[s] ?? s
    );
  }
  getStatusClass(s: string) {
    return (
      {
        COMPLETED: "badge--green",
        VOIDED: "badge--red",
        PENDING: "badge--amber",
      }[s] ?? "badge--gray"
    );
  }
  getCategoryLabel(c: string) {
    return c === "RETAIL" ? "MIN" : "MAY";
  }
}
