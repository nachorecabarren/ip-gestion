import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface BarChartPoint {
  label: string;
  value: number;
}

@Component({
  selector: 'app-bar-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bar-chart.component.html',
  styleUrls: ['./bar-chart.component.scss'],
})
export class BarChartComponent {
  @Input() title = '';
  @Input() color = 'var(--color-blue)';
  @Input() set data(v: BarChartPoint[]) {
    this._data.set(v ?? []);
  }
  @Input() formatValue: (v: number) => string = (v) => `${v}`;

  private _data = signal<BarChartPoint[]>([]);
  hovered = signal<number | null>(null);

  readonly maxValue = computed(() => Math.max(1, ...this._data().map((d) => d.value)));
  readonly points = computed(() =>
    this._data().map((d, i) => ({ ...d, i, heightPct: (d.value / this.maxValue()) * 100 })),
  );

  get chartData() {
    return this._data();
  }

  // Keeps near-zero values visible as a thin sliver instead of disappearing.
  heightFloor(pct: number): number {
    return pct <= 0 ? 0 : Math.max(pct, 3);
  }
}
