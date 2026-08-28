import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { TeamService } from '../../core/services/team.service';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmService } from '../../shared/services/confirm.service';
import { ServiceClientJob, ServiceJobStatus, CatalogModel } from '../../shared/models/models';
import { ImeiScannerComponent } from '../../shared/components/imei-scanner/imei-scanner.component';
import { EscapeCloseDirective } from '../../shared/directives/escape-close.directive';
import { AutoFocusDirective } from '../../shared/directives/auto-focus.directive';
import { confirmDiscard } from '../../shared/utils/confirm-discard';

interface TechnicianOption {
  id: string;
  name: string;
}

@Component({
  selector: 'app-servicio-tecnico',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImeiScannerComponent, EscapeCloseDirective, AutoFocusDirective],
  templateUrl: './servicio-tecnico.component.html',
  styleUrls: ['./servicio-tecnico.component.scss']
})
export class ServicioTecnicoComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private team = inject(TeamService);
  private confirm = inject(ConfirmService);
  auth = inject(AuthService);

  jobs = signal<ServiceClientJob[]>([]);
  total = signal(0);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);
  statusFilter = signal('');
  search = signal('');

  technicians = signal<TechnicianOption[]>([]);
  models = signal<CatalogModel[]>([]);
  showAddModel = signal(false);
  newModelName = signal('');
  addingModel = signal(false);
  form!: FormGroup;

  openModal() {
    this.initForm();
    this.showAddModel.set(false);
    this.showModal.set(true);
  }

  async dismissModal() {
    if (!(await confirmDiscard(this.confirm, this.form))) return;
    this.showModal.set(false);
  }

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
    this.loadTechnicians();
    this.api.getCatalogModels().subscribe(m => this.models.set(m));
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
        this.form.patchValue({ deviceModel: model.name });
        this.showAddModel.set(false);
        this.addingModel.set(false);
      },
      error: () => this.addingModel.set(false),
    });
  }

  private loadTechnicians() {
    this.team.getUsers().subscribe({
      next: users => {
        this.technicians.set(users.filter(u => u.isActive).map(u => ({ id: u.id, name: u.displayName })));
      },
      error: () => {
        this.api.getEntities('TECHNICIAN').subscribe(r => {
          this.technicians.set(r.items.filter(e => e.isActive).map(e => ({ id: e.id, name: e.name })));
        });
      }
    });
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
    const payload = {
      ...this.form.value,
      technicianId: this.form.value.technicianId ? this.form.value.technicianId : null,
    };
    this.api.createServiceJob(payload).subscribe({
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

  onImeiScanned(value: string) {
    this.form.patchValue({ imeiSerial: value });
  }

  get gananciaTotal() {
    return this.jobs().filter(j => j.status === 'DELIVERED')
      .reduce((s, j) => s + j.priceToClientUsd - j.technicianCostUsd, 0);
  }
}
