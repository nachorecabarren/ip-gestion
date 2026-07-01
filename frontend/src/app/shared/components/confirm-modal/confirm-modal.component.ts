import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../services/confirm.service';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  styleUrls: ['./confirm-modal.component.scss'],
  template: `
    <div class="modal-overlay" *ngIf="svc.state() as s" (click)="svc.cancel()">
      <div class="modal modal--sm" (click)="$event.stopPropagation()">
        <div class="modal__header">
          <h2 class="modal__title">Confirmar acción</h2>
          <button class="modal__close" (click)="svc.cancel()">✕</button>
        </div>
        <div class="modal__body">
          <p class="confirm-msg">{{ s.message }}</p>
        </div>
        <div class="modal__footer">
          <button class="btn btn--ghost" (click)="svc.cancel()">Cancelar</button>
          <button class="btn btn--danger" (click)="svc.confirm()">Confirmar</button>
        </div>
      </div>
    </div>
  `
})
export class ConfirmModalComponent {
  svc = inject(ConfirmService);
}
