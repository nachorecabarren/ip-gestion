import { Injectable, signal } from '@angular/core';

interface ConfirmState {
  message: string;
  resolve: (confirmed: boolean) => void;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  readonly state = signal<ConfirmState | null>(null);

  open(message: string): Promise<boolean> {
    return new Promise(resolve => this.state.set({ message, resolve }));
  }

  confirm() {
    this.state()?.resolve(true);
    this.state.set(null);
  }

  cancel() {
    this.state()?.resolve(false);
    this.state.set(null);
  }
}
