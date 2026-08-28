import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { Caja, CashMovement, CashClosingPreview, CashClosing } from '../../shared/models/models';
import { EscapeCloseDirective } from '../../shared/directives/escape-close.directive';
import { AutoFocusDirective } from '../../shared/directives/auto-focus.directive';
import { openIfQueryParam } from '../../shared/utils/open-via-query-param';
import { confirmDiscard } from '../../shared/utils/confirm-discard';

@Component({
  selector: 'app-cajas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, EscapeCloseDirective, AutoFocusDirective],
  templateUrl: './cajas.component.html',
  styleUrls: ['./cajas.component.scss']
})
export class CajasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  cajas = signal<Caja[]>([]);
  movements = signal<CashMovement[]>([]);
  selectedCaja = signal<Caja | null>(null);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  tcBlue = signal(1520);

  form!: FormGroup;

  // Cierre de caja
  showCloseModal = signal(false);
  closePreview = signal<CashClosingPreview | null>(null);
  closeLoading = signal(false);
  closeSubmitting = signal(false);
  closeError = signal('');
  closeForm!: FormGroup;

  showHistory = signal(false);
  closings = signal<CashClosing[]>([]);
  closingsTotal = signal(0);
  closingsPage = signal(1);
  closingsLoading = signal(false);

  ngOnInit() {
    this.initForm();
    this.loadCajas();
    this.api.getTcBlue().subscribe(r => this.tcBlue.set(r.rate));
    openIfQueryParam(this.route, this.router, 'nueva', () => this.openModal());
  }

  openModal() {
    this.initForm();
    const defaultCajaId = this.selectedCaja()?.id
      ?? this.cajas().find(c => c.isDefault)?.id
      ?? this.cajas()[0]?.id
      ?? '';
    this.form.patchValue({ cajaId: defaultCajaId });
    this.showModal.set(true);
  }

  async dismissModal() {
    if (!(await confirmDiscard(this.confirm, this.form))) return;
    this.showModal.set(false);
  }

  initForm() {
    this.form = this.fb.group({
      cajaId: ['', Validators.required],
      type: ['INCOME', Validators.required],
      method: ['USD_CASH', Validators.required],
      currency: ['USD'],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      exchangeRateUsd: [1],
      detail: ['', Validators.required],
    });

    this.form.get('method')?.valueChanges.subscribe(m => {
      const isArs = ['ARS_CASH', 'ARS_TR', 'MERCADOPAGO'].includes(m);
      this.form.patchValue({ currency: isArs ? 'ARS' : 'USD', exchangeRateUsd: isArs ? this.tcBlue() : 1 });
    });
  }

  loadCajas() {
    this.api.getCajas().subscribe(c => {
      this.cajas.set(c);
      if (c.length > 0 && !this.selectedCaja()) {
        this.selectedCaja.set(c[0]);
        this.form.patchValue({ cajaId: c[0].id });
        this.loadMovements(c[0].id);
      }
    });
  }

  loadMovements(cajaId: string) {
    this.loading.set(true);
    this.api.getCashMovements(cajaId).subscribe(m => {
      this.movements.set(m);
      this.loading.set(false);
    });
  }

  selectCaja(c: Caja) {
    this.selectedCaja.set(c);
    this.form.patchValue({ cajaId: c.id });
    this.loadMovements(c.id);
    this.showHistory.set(false);
  }

  openCloseModal() {
    const caja = this.selectedCaja();
    if (!caja) return;
    this.closeForm = this.fb.group({
      countedUsdCash: [0, [Validators.required, Validators.min(0)]],
      countedArsCash: [0, [Validators.required, Validators.min(0)]],
      notes: [''],
    });
    this.closeError.set('');
    this.closePreview.set(null);
    this.closeLoading.set(true);
    this.showCloseModal.set(true);
    this.api.getClosingPreview(caja.id).subscribe({
      next: (p) => {
        this.closePreview.set(p);
        this.closeForm.patchValue({ countedUsdCash: p.expectedUsdCash, countedArsCash: p.expectedArsCash });
        this.closeLoading.set(false);
      },
      error: () => this.closeLoading.set(false),
    });
  }

  async dismissCloseModal() {
    if (!(await confirmDiscard(this.confirm, this.closeForm))) return;
    this.showCloseModal.set(false);
  }

  get diffUsdCash() {
    return (this.closeForm?.get('countedUsdCash')?.value ?? 0) - (this.closePreview()?.expectedUsdCash ?? 0);
  }
  get diffArsCash() {
    return (this.closeForm?.get('countedArsCash')?.value ?? 0) - (this.closePreview()?.expectedArsCash ?? 0);
  }

  submitClose() {
    const caja = this.selectedCaja();
    if (!caja || this.closeForm.invalid) { this.closeForm.markAllAsTouched(); return; }
    this.closeSubmitting.set(true);
    this.closeError.set('');
    this.api.closeCaja({ cajaId: caja.id, ...this.closeForm.value }).subscribe({
      next: () => {
        this.showCloseModal.set(false);
        this.closeSubmitting.set(false);
        this.loadCajas();
        this.loadMovements(caja.id);
        if (this.showHistory()) this.loadClosings(caja.id);
      },
      error: (e) => {
        this.closeError.set(e?.error?.error ?? e?.error?.title ?? `Error ${e?.status}`);
        this.closeSubmitting.set(false);
      },
    });
  }

  toggleHistory() {
    this.showHistory.update((v) => !v);
    const caja = this.selectedCaja();
    if (this.showHistory() && caja) {
      this.closingsPage.set(1);
      this.loadClosings(caja.id);
    }
  }

  loadClosings(cajaId: string) {
    this.closingsLoading.set(true);
    this.api.getClosings(cajaId, this.closingsPage(), 20).subscribe({
      next: (r) => { this.closings.set(r.items); this.closingsTotal.set(r.total); this.closingsLoading.set(false); },
      error: () => this.closingsLoading.set(false),
    });
  }

  submit() {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.api.registerCashMovement(this.form.value).subscribe({
      next: () => {
        this.showModal.set(false);
        this.loadCajas();
        if (this.selectedCaja()) this.loadMovements(this.selectedCaja()!.id);
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false)
    });
  }

  get totalUsd() {
    return (this.selectedCaja()?.balanceUsdCash ?? 0) + (this.selectedCaja()?.balanceUsdt ?? 0);
  }

  getTypeClass(t: string) {
    return ['INCOME', 'SALE'].includes(t) ? 'text-green' : 'text-red';
  }
  getTypeSign(t: string) { return ['INCOME', 'SALE'].includes(t) ? '+' : '-'; }
  getTypeLabel(t: string) {
    return ({ SALE: 'Venta', EXPENSE: 'Gasto', INCOME: 'Ingreso', PURCHASE: 'Compra' } as Record<string,string>)[t] ?? t;
  }
  getMethodLabel(m: string) {
    return ({ USD_CASH: 'Efectivo USD', ARS_CASH: 'Efectivo ARS', ARS_TR: 'Transferencia ARS', USDT: 'USDT', MERCADOPAGO: 'MercadoPago' } as Record<string,string>)[m] ?? m;
  }
}
