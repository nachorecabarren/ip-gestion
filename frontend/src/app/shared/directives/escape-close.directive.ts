import { Directive, EventEmitter, HostListener, Output } from '@angular/core';

/**
 * Attach to a modal overlay to close it on Escape, regardless of where
 * focus currently is (listens on document, not just the host element).
 */
@Directive({
  selector: '[escapeClose]',
  standalone: true,
})
export class EscapeCloseDirective {
  @Output() escapeClose = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscape() {
    this.escapeClose.emit();
  }
}
