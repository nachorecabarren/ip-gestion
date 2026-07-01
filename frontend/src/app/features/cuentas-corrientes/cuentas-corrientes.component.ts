import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { EntityBalance } from '../../shared/models/models';

@Component({
  selector: 'app-cuentas-corrientes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './cuentas-corrientes.component.html',
  styleUrls: ['./cuentas-corrientes.component.scss']
})
export class CuentasCorrientesComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);

  balances = signal<EntityBalance[]>([]);
  total = signal(0);
  loading = signal(true);
  showModal = signal(false);
  selectedEntity = signal<EntityBalance | null>(null);
  filterMode = signal('');

  form!: FormGroup;

  ngOnInit() {
    this.initForm();
    this.load();
  }

  initForm() {
    this.form = this.fb.group({
      entityId: [''],
      amountUsd: [0, [Validators.required, Validators.min(0.01)]],
      method: ['USD_CASH', Validators.required],
      currency: ['USD'],
      exchangeRateUsd: [1],
      notes: [''],
    });
  }

  load() {
    this.loading.set(true);
    this.api.getBalances(undefined, this.filterMode() || undefined).subscribe({
      next: r => { this.balances.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
  }

  openPayment(b: EntityBalance) {
    this.selectedEntity.set(b);
    this.form.patchValue({ entityId: b.entityId, amountUsd: Math.abs(b.balanceUsd) });
    this.showModal.set(true);
  }

  submit() {
    if (this.form.invalid) return;
    const v = this.form.value;
    // Sign: if balance > 0 (nos deben), recibimos pago → positivo. Si < 0 (debemos), pagamos → negativo
    const adj = this.selectedEntity()!.balanceUsd > 0 ? -Math.abs(v.amountUsd) : Math.abs(v.amountUsd);
    this.api.recordPayment({ ...v, amountUsd: adj }).subscribe(() => {
      this.showModal.set(false);
      this.load();
    });
  }

  getBalanceClass(b: number) { return b > 0 ? 'text-green' : b < 0 ? 'text-red' : ''; }
  getBalanceLabel(b: number) { return b > 0 ? 'A cobrar' : b < 0 ? 'A pagar' : 'Al día'; }
  getEntityTypeLabel(t: string) {
    return ({ PROVIDER: 'Proveedor', CLIENT: 'Cliente', TECHNICIAN: 'Técnico', COURIER: 'Mensajería' } as Record<string,string>)[t] ?? t;
  }
}
