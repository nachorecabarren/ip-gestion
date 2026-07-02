import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnDestroy, Output, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Html5Qrcode } from 'html5-qrcode';

@Component({
  selector: 'app-imei-scanner',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  styleUrl: './imei-scanner.component.scss',
  template: `
    <div class="imei-scanner">
      <label class="imei-scanner__label" *ngIf="label">{{ label }}</label>
      <div class="imei-scanner__input-row">
        <input
          type="text"
          class="form-control"
          [placeholder]="placeholder"
          [formControl]="manualControl"
          (keyup.enter)="submitManual()"
        />
        <button type="button" class="btn btn--primary btn--sm" (click)="openModal()">📷 Escanear</button>
      </div>

      <div class="imei-scanner__feedback" *ngIf="feedback()">
        <span class="badge" [ngClass]="feedbackType() === 'error' ? 'badge--red' : 'badge--green'">{{ feedback() }}</span>
      </div>

      <div class="imei-scanner__confirmation" *ngIf="value()">
        <span class="badge badge--green">IMEI cargado: {{ value() }}</span>
      </div>

      <div class="modal-overlay" *ngIf="isOpen()" (click)="closeModal()">
        <div class="modal modal--wide" style="max-width: 720px" (click)="$event.stopPropagation()">
          <div class="modal__header">
            <div>
              <h2 class="modal__title">Escanear IMEI</h2>
              <p class="modal__sub">Apuntá la cámara al código de barras del iPhone</p>
            </div>
            <button class="modal__close" type="button" (click)="closeModal()">✕</button>
          </div>

          <div class="modal__body">
            <div class="imei-scanner__camera-frame" *ngIf="!cameraError()">
              <div [id]="readerId" class="imei-scanner__reader"></div>
              <div class="imei-scanner__guide">
                <span class="imei-scanner__corner imei-scanner__corner--tl"></span>
                <span class="imei-scanner__corner imei-scanner__corner--tr"></span>
                <span class="imei-scanner__corner imei-scanner__corner--bl"></span>
                <span class="imei-scanner__corner imei-scanner__corner--br"></span>
              </div>
              <div class="imei-scanner__scan-status">
                <span class="imei-scanner__dot"></span> Buscando código...
              </div>
            </div>

            <div class="imei-scanner__manual" *ngIf="cameraError()">
              <div class="imei-scanner__manual-title">No se pudo iniciar la cámara</div>
              <p class="imei-scanner__manual-text">{{ cameraError() }}</p>
              <input type="text" class="form-control" [formControl]="manualControl" placeholder="Ingresá los 15 dígitos" />
              <button type="button" class="btn btn--primary" style="margin-top: 10px" (click)="submitManual()">Confirmar IMEI</button>
            </div>

            <div class="imei-scanner__hint">
              Si el navegador no permite usar la cámara, podés ingresar el IMEI manualmente.
            </div>
          </div>

          <div class="modal__footer">
            <button type="button" class="btn btn--ghost" (click)="closeModal()">Cancelar</button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ImeiScannerComponent implements OnDestroy {
  private html5QrCode: Html5Qrcode | null = null;
  private isStopping = false;
  readonly readerId = `imei-scanner-reader-${Math.random().toString(36).slice(2, 10)}`;

  @Input() label = 'IMEI / Serie';
  @Input() placeholder = 'Escanear o ingresar manualmente...';
  @Output() imeiScanned = new EventEmitter<string>();

  manualControl = new FormControl('', [Validators.required, Validators.pattern(/^\d{15}$/)]);
  isOpen = signal(false);
  feedback = signal('');
  feedbackType = signal<'success' | 'error'>('success');
  cameraError = signal('');
  value = signal('');

  openModal() {
    this.feedback.set('');
    this.cameraError.set('');
    this.isOpen.set(true);
    requestAnimationFrame(() => {
      setTimeout(() => void this.startScanner(), 250);
    });
  }

  async startScanner() {
    if (!this.isOpen()) return;
    if (this.html5QrCode) return;

    const elementId = this.readerId;
    const element = document.getElementById(elementId);
    if (!element) return;

    try {
      await this.stopScanner();
      element.innerHTML = '';
      const scanner = new Html5Qrcode(elementId, { verbose: false });
      this.html5QrCode = scanner;

      await scanner.start(
        { facingMode: 'environment' },
        { fps: 10, qrbox: { width: 220, height: 220 }, aspectRatio: 1.0 },
        (decodedText) => this.handleScan(decodedText),
        () => undefined,
      );
    } catch (error) {
      this.cameraError.set('No se pudo acceder a la cámara. Podés continuar con el ingreso manual.');
      await this.stopScanner();
    }
  }

  private handleScan(decodedText: string) {
    const normalized = decodedText.replace(/\s+/g, '').trim();
    const cleaned = normalized.replace(/[^\d]/g, '').slice(0, 15);

    if (!/^\d{15}$/.test(cleaned)) {
      this.feedback.set('El código escaneado no parece un IMEI válido. Intentá nuevamente.');
      this.feedbackType.set('error');
      return;
    }

    this.value.set(cleaned);
    this.manualControl.setValue(cleaned, { emitEvent: false });
    this.feedback.set('IMEI capturado correctamente.');
    this.feedbackType.set('success');
    this.imeiScanned.emit(cleaned);
    this.closeModal();
  }

  submitManual() {
    const value = (this.manualControl.value || '').replace(/\s+/g, '').trim();
    if (!/^\d{15}$/.test(value)) {
      this.feedback.set('Ingresá un IMEI válido de 15 dígitos numéricos.');
      this.feedbackType.set('error');
      return;
    }

    this.value.set(value);
    this.feedback.set('IMEI cargado correctamente.');
    this.feedbackType.set('success');
    this.imeiScanned.emit(value);
    this.closeModal();
  }

  closeModal() {
    this.isOpen.set(false);
    this.stopScanner();
  }

  reset() {
    this.value.set('');
    this.feedback.set('');
    this.feedbackType.set('success');
    this.cameraError.set('');
    this.manualControl.setValue('', { emitEvent: false });
  }

  private async stopScanner() {
    if (this.isStopping) return;
    const qrCode = this.html5QrCode;
    if (!qrCode) return;

    this.isStopping = true;
    try {
      if (qrCode.isScanning) {
        await qrCode.stop();
      }
      qrCode.clear();
    } catch {
      // Safari/quirks can throw when the scanner is already stopped or unavailable.
    } finally {
      if (this.html5QrCode === qrCode) {
        this.html5QrCode = null;
      }
      this.isStopping = false;
    }
  }

  ngOnDestroy() {
    this.closeModal();
  }
}
