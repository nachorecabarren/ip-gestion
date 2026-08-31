import { Component, OnInit, inject, signal, computed } from "@angular/core";
import { ConfirmService } from "../../shared/services/confirm.service";
import { AuthService } from "../../core/services/auth.service";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
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
  SaleItem,
  StockItem,
  StockBulk,
  Entity,
  CatalogModel,
  PaymentMethod,
  SaleCategory,
} from "../../shared/models/models";
import { ImeiScannerComponent } from "../../shared/components/imei-scanner/imei-scanner.component";
import { StockItemSelectComponent } from "../../shared/components/stock-item-select/stock-item-select.component";
import { EscapeCloseDirective } from "../../shared/directives/escape-close.directive";
import { AutoFocusDirective } from "../../shared/directives/auto-focus.directive";
import { openIfQueryParam } from "../../shared/utils/open-via-query-param";
import { confirmDiscard } from "../../shared/utils/confirm-discard";

@Component({
  selector: "app-ventas",
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, ImeiScannerComponent, StockItemSelectComponent, EscapeCloseDirective, AutoFocusDirective],
  templateUrl: "./ventas.component.html",
  styleUrls: ["./ventas.component.scss"],
})
export class VentasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
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
  bulkStock = signal<StockBulk[]>([]);
  models = signal<CatalogModel[]>([]);
  tcBlue = signal(1520);
  tradeInKpis = signal<{ ventasConCanje: number; porcentajeCanje: number } | null>(null);

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
      .getStockItems("AVAILABLE", undefined, undefined, 1, 500)
      .subscribe((r) => this.availableStock.set(r.items));
    this.api.getStockBulk().subscribe((b) => this.bulkStock.set(b));
    this.api.getCatalogModels().subscribe((m) => this.models.set(m));
    // Fuente única de verdad: el % de canje se calcula en el servidor sobre
    // todas las ventas del mes, no sobre la página parcial cargada acá.
    this.api.getDashboardKpis("month").subscribe((k) => this.tradeInKpis.set(k));

    openIfQueryParam(this.route, this.router, "nueva", () => this.openNewSale());
  }

  initForm() {
    this.saleForm = this.fb.group({
      saleDate: [new Date().toISOString().split("T")[0], Validators.required],
      saleCategory: ["RETAIL"],
      entityId: [null],
      retailClientName: [""],
      retailClientPhone: [""],
      invoiceEmail: [""],
      // Arranca apagado: para accesorios no tiene sentido pedir email. Se
      // prende solo cuando se agrega un equipo (ver onStockSelect), donde sí
      // es obligatorio.
      sendInvoiceEmail: [false],
      isConsumerFinal: [true],
      warrantyDays: [90, [Validators.required, Validators.min(0)]],
      notes: [""],
      items: this.fb.array([]),
      payments: this.fb.array([]),
      tradeInEnabled: [false],
      tradeInModelId: [""],
      tradeInImei: [""],
      tradeInColor: [""],
      tradeInStorage: [null],
      tradeInBattery: [null],
      tradeInCondition: ["USED"],
      tradeInValue: [0],
      tradeInSuggestedPrice: [0],
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
  get canjesCount() { return this.tradeInKpis()?.ventasConCanje ?? 0; }
  get canjesPct() { return this.tradeInKpis()?.porcentajeCanje ?? 0; }

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

  async dismissModal() {
    if (!(await confirmDiscard(this.confirm, this.saleForm))) return;
    this.closeModal();
  }

  nextStep() {
    const error = this.validateStep(this.wizardStep());
    if (error) {
      this.submitError.set(error);
      this.saleForm.markAllAsTouched();
      return;
    }
    this.submitError.set('');
    if (this.wizardStep() < 4) this.wizardStep.update((s) => s + 1);
  }
  prevStep() {
    this.submitError.set('');
    if (this.wizardStep() > 1) this.wizardStep.update((s) => s - 1);
  }

  isValidEmail(value: string) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
  }

  /**
   * Una venta de solo accesorios (funda, cable, etc.) no necesita datos del cliente.
   * Ojo: cada ítem nuevo arranca en type 'EQUIPMENT' por defecto aunque todavía no
   * se haya elegido nada, así que solo cuenta si además tiene un equipo seleccionado.
   */
  hasEquipmentItem(): boolean {
    return this.items.controls.some(c => c.get('type')?.value === 'EQUIPMENT' && !!c.get('stockItemId')?.value);
  }

  /** Datos obligatorios por paso: equipo (1), cliente (2, solo si hay equipos) y, si corresponde, email de factura. */
  validateStep(step: number): string | null {
    const v = this.saleForm.getRawValue();
    if (step === 1) {
      if (this.items.length === 0) return 'Agregá al menos un ítem a la venta.';
      for (const c of this.items.controls) {
        const isAccessory = c.get('type')?.value === 'ACCESSORY';
        if (isAccessory && !c.get('stockBulkId')?.value) return 'Seleccioná un accesorio para cada ítem agregado.';
        if (!isAccessory && !c.get('stockItemId')?.value) return 'Seleccioná un equipo para cada ítem agregado.';
        if (!(Number(c.get('priceUsd')?.value) > 0)) return 'Cada ítem debe tener un precio válido.';
      }
      return null;
    }
    if (step === 2) {
      const requiresClientData = this.hasEquipmentItem();
      if (v.isConsumerFinal) {
        if (requiresClientData && !v.retailClientName?.trim()) return 'Ingresá el nombre del cliente.';
      } else if (!v.entityId) {
        return 'Seleccioná un cliente.';
      }
      if ((requiresClientData || v.sendInvoiceEmail) && !this.isValidEmail(v.invoiceEmail || '')) {
        return requiresClientData
          ? 'Ingresá un email válido — es obligatorio en ventas con equipos.'
          : 'Ingresá un email válido para enviar la factura.';
      }
      return null;
    }
    if (step === 3) {
      if (v.tradeInEnabled && !v.tradeInModelId) return 'Seleccioná el modelo del equipo a canjear.';
      return null;
    }
    return null;
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

  onEntitySelect(event: Event) {
    const id = (event.target as HTMLSelectElement).value;
    const entity = this.entities().find((e) => e.id === id);
    if (entity?.email) this.saleForm.patchValue({ invoiceEmail: entity.email });
  }

  onTradeInImeiScanned(value: string) {
    this.saleForm.patchValue({ tradeInImei: value });
  }

  addItem() {
    this.itemFilters.update(arr => [...arr, '']);
    this.items.push(
      this.fb.group({
        type: ["EQUIPMENT"],
        stockItemId: [null],
        stockBulkId: [""],
        quantity: [1, [Validators.min(1)]],
        priceUsd: [0, [Validators.required, Validators.min(0.01)]],
      }),
    );
  }

  setItemType(i: number, type: "EQUIPMENT" | "ACCESSORY") {
    const group = this.items.at(i);
    if (group.get("type")?.value === type) return;
    group.patchValue({ type, stockItemId: null, stockBulkId: "", priceUsd: 0, quantity: 1 });
    this.setItemFilter(i, "");
    this.updateTotals();
  }

  onBulkSelect(i: number, bulkId: string | null) {
    const bulk = this.bulkStock().find((b) => b.id === bulkId);
    if (bulk) {
      this.items.at(i).patchValue({ priceUsd: bulk.suggestedPriceUsd });
      this.updateTotals();
    }
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

  onStockSelect(index: number, id: string | null) {
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
      // Las ventas de equipos siempre piden email — activamos el envío de
      // factura por defecto en vez de dejarlo como un campo obligatorio "mudo".
      this.saleForm.patchValue({ sendInvoiceEmail: true });
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
    const total = Math.max(this.totalPago(), 0);

    for (const step of [1, 2, 3]) {
      const error = this.validateStep(step);
      if (error) {
        this.submitError.set(error);
        this.saleForm.markAllAsTouched();
        this.wizardStep.set(step);
        return;
      }
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
      retailClientInstagram: null,
      sendInvoiceEmail: !!raw.sendInvoiceEmail,
      invoiceEmail: raw.sendInvoiceEmail ? raw.invoiceEmail || null : null,
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
            modelId: raw.tradeInModelId,
            imeiSerial: raw.tradeInImei || null,
            color: raw.tradeInColor || null,
            storageGb: raw.tradeInStorage,
            batteryPct: raw.tradeInBattery,
            condition: raw.tradeInCondition,
            valueUsd: raw.tradeInValue || 0,
            suggestedPriceUsd: raw.tradeInSuggestedPrice || 0,
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

  getSaleItemImeiList(items: SaleItem[]) {
    return items.filter(item => item.imeiSerial).map(item => item.imeiSerial!);
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
