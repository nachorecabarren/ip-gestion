import { Injectable, signal } from '@angular/core';

export type AppTheme = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<AppTheme>('light');
  readonly isDark = signal(false);

  constructor() {
    const saved = (localStorage.getItem('ip-gestion-theme') as AppTheme | null) ?? null;
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const initial = saved ?? (systemDark ? 'dark' : 'light');
    this.applyTheme(initial);
  }

  toggleTheme() {
    this.applyTheme(this.isDark() ? 'light' : 'dark');
  }

  applyTheme(theme: AppTheme) {
    this.theme.set(theme);
    this.isDark.set(theme === 'dark');
    document.documentElement.setAttribute('data-theme', theme);
    document.documentElement.style.colorScheme = theme;
    localStorage.setItem('ip-gestion-theme', theme);
  }
}
