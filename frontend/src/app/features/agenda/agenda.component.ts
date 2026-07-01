import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { ConfirmService } from '../../shared/services/confirm.service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { CalendarEvent } from '../../shared/models/models';

@Component({
  selector: 'app-agenda',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './agenda.component.html',
  styleUrls: ['./agenda.component.scss']
})
export class AgendaComponent implements OnInit {
  private api = inject(ApiService);
  private fb = inject(FormBuilder);
  private confirm = inject(ConfirmService);

  events = signal<CalendarEvent[]>([]);
  loading = signal(true);
  showModal = signal(false);
  submitting = signal(false);

  today = new Date();
  currentYear = signal(this.today.getFullYear());
  currentMonth = signal(this.today.getMonth() + 1);

  form!: FormGroup;

  readonly monthNames = ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
    'Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];

  readonly eventTypes = [
    { value: 'RESERVATION', label: '🔖 Retiro reserva' },
    { value: 'MEETING', label: '🤝 Reunión' },
    { value: 'PAYMENT', label: '💰 Pago / cobro' },
    { value: 'OTHER', label: '📌 Otro' },
  ];

  ngOnInit() {
    this.initForm();
    this.load();
  }

  initForm() {
    const now = new Date();
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      startTime: [this.formatDateTimeLocal(now), Validators.required],
      endTime: [this.formatDateTimeLocal(new Date(now.getTime() + 3600000)), Validators.required],
      type: ['MEETING', Validators.required],
    });
  }

  load() {
    this.loading.set(true);
    this.api.getCalendarEvents(this.currentYear(), this.currentMonth()).subscribe({
      next: e => { this.events.set(e); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  prevMonth() {
    if (this.currentMonth() === 1) { this.currentMonth.set(12); this.currentYear.update(y => y - 1); }
    else this.currentMonth.update(m => m - 1);
    this.load();
  }

  nextMonth() {
    if (this.currentMonth() === 12) { this.currentMonth.set(1); this.currentYear.update(y => y + 1); }
    else this.currentMonth.update(m => m + 1);
    this.load();
  }

  eventsForDay(day: number): CalendarEvent[] {
    return this.events().filter(e => new Date(e.startTime).getDate() === day);
  }

  get calendarDays(): (number | null)[] {
    const firstDay = new Date(this.currentYear(), this.currentMonth() - 1, 1).getDay();
    const daysInMonth = new Date(this.currentYear(), this.currentMonth(), 0).getDate();
    const blanks = Array(firstDay).fill(null);
    const days = Array.from({ length: daysInMonth }, (_, i) => i + 1);
    return [...blanks, ...days];
  }

  isToday(day: number) {
    return day === this.today.getDate() &&
      this.currentMonth() === this.today.getMonth() + 1 &&
      this.currentYear() === this.today.getFullYear();
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting.set(true);
    const v = this.form.value;
    const dto = {
      title: v.title,
      description: v.description,
      startTime: new Date(v.startTime).toISOString(),
      endTime: new Date(v.endTime).toISOString(),
      type: v.type,
    };
    this.api.createCalendarEvent(dto).subscribe({
      next: () => { this.showModal.set(false); this.load(); this.submitting.set(false); },
      error: () => this.submitting.set(false)
    });
  }

  async deleteEvent(id: string) {
    if (!await this.confirm.open('¿Eliminar este evento de la agenda?')) return;
    this.api.deleteCalendarEvent(id).subscribe(() => this.load());
  }

  getTypeEmoji(type: string) {
    return ({ RESERVATION: '🔖', MEETING: '🤝', PAYMENT: '💰', OTHER: '📌' })[type] ?? '📌';
  }

  getTypeClass(type: string) {
    return ({ RESERVATION: 'event--amber', MEETING: 'event--blue', PAYMENT: 'event--green', OTHER: 'event--gray' })[type] ?? 'event--gray';
  }

  private formatDateTimeLocal(d: Date) {
    return d.toISOString().slice(0, 16);
  }
}