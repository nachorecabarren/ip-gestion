import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { ServiceClientJob, Entity, ServiceJobStatus } from '../../shared/models/models';

@Component({
  selector: 'app-servicio-tecnico',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './servicio-tecnico.component.html',
  styleUrls: ['./servicio-tecnico.component.scss']
})
export class ServicioTecnicoComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);

  jobs = signal<ServiceClientJob[]>([]);
  total = signal(0);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  statusFilter = signal('');
  search = signal('');

  technicians = signal<Entity[]>([]);
  form!: FormGroup;

  readonly statusFlow: { value: ServiceJobStatus; label: string; class: string }[] = [
    { value: 'OPEN', label: 'Abierto', class: 'badge--blue' },
    { value: 'IN_REPAIR', label: 'En reparación', class: 'badge--amber' },
    { value: 'READY_FOR_DELIVERY', label: 'Listo', class: 'badge--green' },
    { value: 'DELIVERED', label: 'Entregado', class: 'badge--gray' },
    { value: 'CANCELLED', label: 'Cancelado', class: 'badge--red' },
  ];

  ngOnInit() {
    this.initForm();
    this.load();
    this.api.getEntities('TECHNICIAN').subscribe(r => this.technicians.set(r.items));
  }

  initForm() {
    this.form = this.fb.group({
      retailClientName: ['', Validators.required],
      retailClientPhone: [''],
      deviceModel: [''],
      imeiSerial: [''],
      issueDescription: ['', Validators.required],
      technicianId: [null],
      priceToClientUsd: [0, [Validators.required, Validators.min(0)]],
      technicianCostUsd: [0, [Validators.min(0)]],
      depositMethod: [null],
      depositAmount: [0],
      limitDate: [null],
    });
  }

  load() {
    this.loading.set(true);
    this.api.getServiceJobs(this.statusFilter() as any || undefined, this.search() || undefined).subscribe({
      next: r => { this.jobs.set(r.items); this.total.set(r.total); this.loading.set(false); }
    });
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    this.api.createServiceJob(this.form.value).subscribe({
      next: () => { this.showModal.set(false); this.load(); this.submitting.set(false); },
      error: () => this.submitting.set(false)
    });
  }

  updateStatus(id: string, status: ServiceJobStatus) {
    this.api.updateServiceJobStatus(id, status).subscribe(() => this.load());
  }

  nextStatus(current: ServiceJobStatus): ServiceJobStatus | null {
    const flow: ServiceJobStatus[] = ['OPEN', 'IN_REPAIR', 'READY_FOR_DELIVERY', 'DELIVERED'];
    const idx = flow.indexOf(current);
    return idx >= 0 && idx < flow.length - 1 ? flow[idx + 1] : null;
  }

  getStatusObj(s: string) { return this.statusFlow.find(x => x.value === s); }

  isOverdue(job: ServiceClientJob) {
    if (!job.limitDate || ['DELIVERED', 'CANCELLED', 'CLOSED'].includes(job.status)) return false;
    return new Date(job.limitDate) < new Date();
  }

  get gananciaTotal() {
    return this.jobs().filter(j => j.status === 'DELIVERED')
      .reduce((s, j) => s + j.priceToClientUsd - j.technicianCostUsd, 0);
  }
}
