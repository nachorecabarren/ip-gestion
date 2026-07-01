import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './sidebar/sidebar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent],
  template: `
    <div class="layout">
      <app-sidebar />
      <main class="layout__main">
        <div class="layout__content">
          <router-outlet />
        </div>
      </main>
    </div>
  `,
  styles: [`
    .layout { display: flex; min-height: 100vh; background: var(--color-bg); }
    .layout__main { flex: 1; overflow-y: auto; }
    .layout__content { padding: 24px; max-width: 1400px; }
  `]
})
export class MainLayoutComponent {}
