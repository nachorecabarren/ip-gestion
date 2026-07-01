import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Caja, CashMovement } from '../../shared/models/models';

@Component({
  selector: 'app-cajas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './cajas.component.html',
  styleUrls: ['./cajas.component.scss']
})
export class CajasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);

  cajas = signal<Caja[]>([]);
  movements = signal<CashMovement[]>([]);
  selectedCaja = signal<Caja | null>(null);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  tcBlue = signal(1520);

  form!: FormGroup;

  ngOnInit() {
    this.initForm();
    this.loadCajas();
    this.api.getTcBlue().subscribe(r => this.tcBlue.set(r.rate));
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
