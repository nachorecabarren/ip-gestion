import { AfterViewInit, Directive, ElementRef } from '@angular/core';

/**
 * Focuses the host element once it's rendered. Meant for the first field of
 * a modal — since modal content is created fresh via *ngIf each time it
 * opens, this re-runs (and re-focuses) on every open.
 */
@Directive({
  selector: '[autoFocus]',
  standalone: true,
})
export class AutoFocusDirective implements AfterViewInit {
  constructor(private el: ElementRef<HTMLElement>) {}

  ngAfterViewInit() {
    setTimeout(() => this.el.nativeElement.focus());
  }
}
