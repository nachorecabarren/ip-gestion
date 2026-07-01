import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { AuthService } from '../../core/services/auth.service';
import { Reservation, Entity, StockItem } from '../../shared/models/models';

@Component({
  selector: 'app-reservas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reservas.component.html',
  styleUrls: ['./reservas.component.scss']
})
export class ReservasComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);
  auth = inject(AuthService);

  reservations = signal<Reservation[]>([]);
  total = signal(0);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  statusFilter = signal('ACTIVE');

  clients = signal<Entity[]>([]);
  availableStock = signal<StockItem[]>([]);

  form!: FormGroup;

  ngOnInit() {
    this.initForm();
    this.load();
    this.api.getEntities('CLIENT').subscribe(r => this.clients.set(r.items));
    this.api.getStockItems('AVAILABLE').subscribe(r => this.availableStock.set(r.items));
  }

  initForm() {
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
  }

  load() {
    this.loading.set(true);
    this.api.getReservations(this.statusFilter() as any || undefined).subscribe({
      next: r => { this.reservations.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
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

  getStatusClass(s: string) {
    return ({ ACTIVE: 'badge--amber', SOLD: 'badge--green', CANCELLED: 'badge--red' })[s] ?? 'badge--gray';
  }
  getStatusLabel(s: string) {
    return ({ ACTIVE: 'Activa', SOLD: 'Concretada', CANCELLED: 'Cancelada' })[s] ?? s;
  }

  isPastPickup(d: string) { return new Date(d) < new Date(); }
}
