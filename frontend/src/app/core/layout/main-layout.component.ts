import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent],
  template: `
    <div class="layout">
      <div class="layout__backdrop" *ngIf="isMobile() && isMobileMenuOpen()" (click)="closeMobileMenu()"></div>

      <div class="layout__sidebar" [class.layout__sidebar--mobile-open]="isMobile() && isMobileMenuOpen()">
        <app-sidebar (itemClicked)="closeMobileMenu()" />
      </div>

      <main class="layout__main">
        <header class="layout__topbar" *ngIf="isMobile()">
          <button class="layout__menu-btn" type="button" (click)="toggleMobileMenu()" [attr.aria-expanded]="isMobileMenuOpen()" aria-label="Abrir menú">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="3" y1="6" x2="21" y2="6"/>
              <line x1="3" y1="12" x2="21" y2="12"/>
              <line x1="3" y1="18" x2="21" y2="18"/>
            </svg>
          </button>
          <div class="layout__topbar-title">iP Gestión</div>
          <div class="layout__topbar-spacer"></div>
        </header>

        <div class="layout__content">
          <router-outlet />
        </div>
      </main>
    </div>
  `,
  styles: [`
    .layout { display: flex; min-height: 100vh; background: var(--color-bg); position: relative; }
    .layout__sidebar { position: sticky; top: 0; height: 100vh; flex-shrink: 0; z-index: 20; }
    .layout__main { flex: 1; min-width: 0; overflow-y: auto; }
    .layout__content { padding: 24px; max-width: 1400px; margin: 0 auto; width: 100%; }
    .layout__backdrop { display: none; }
    .layout__topbar { display: none; }

    @media (max-width: 900px) {
      .layout { flex-direction: column; }
      .layout__sidebar {
        position: fixed; left: 0; top: 0; bottom: 0; height: 100vh;
        transform: translateX(-100%); transition: transform 0.2s ease;
        box-shadow: var(--shadow-lg); z-index: 40;
      }
      .layout__sidebar--mobile-open { transform: translateX(0); }
      .layout__backdrop {
        display: block; position: fixed; inset: 0; background: rgba(15, 23, 42, 0.35);
        z-index: 30;
      }
      .layout__main { min-height: 100vh; }
      .layout__topbar {
        display: flex; align-items: center; gap: 12px; padding: 14px 16px;
        position: sticky; top: 0; z-index: 12; background: rgba(248, 250, 252, 0.95);
        backdrop-filter: blur(8px); border-bottom: 1px solid var(--color-border);
      }
      .layout__menu-btn {
        display: flex; align-items: center; justify-content: center; width: 40px; height: 40px;
        border: 1px solid var(--color-border); border-radius: 10px; background: #fff; color: var(--color-text-primary);
        cursor: pointer; flex-shrink: 0;
      }
      .layout__menu-btn svg { width: 18px; height: 18px; }
      .layout__topbar-title { font-weight: 700; font-size: 15px; }
      .layout__topbar-spacer { flex: 1; }
      .layout__content { padding: 16px; }
    }
  `]
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  isMobileMenuOpen = signal(false);
  isMobile = signal(false);

  private mediaQuery = window.matchMedia('(max-width: 900px)');
  private readonly onMediaChange = (event: MediaQueryList | MediaQueryListEvent) => {
    this.isMobile.set(event.matches);
    if (!event.matches) this.isMobileMenuOpen.set(false);
  };

  ngOnInit() {
    this.onMediaChange(this.mediaQuery);
    this.mediaQuery.addEventListener?.('change', this.onMediaChange as EventListener);
    (this.mediaQuery as MediaQueryList & { addListener?: (listener: (event: MediaQueryListEvent) => void) => void }).addListener?.(this.onMediaChange as unknown as (event: MediaQueryListEvent) => void);
  }

  ngOnDestroy() {
    this.mediaQuery.removeEventListener?.('change', this.onMediaChange as EventListener);
    (this.mediaQuery as MediaQueryList & { removeListener?: (listener: (event: MediaQueryListEvent) => void) => void }).removeListener?.(this.onMediaChange as unknown as (event: MediaQueryListEvent) => void);
  }

  toggleMobileMenu() { this.isMobileMenuOpen.set(!this.isMobileMenuOpen()); }
  closeMobileMenu() { this.isMobileMenuOpen.set(false); }
}
