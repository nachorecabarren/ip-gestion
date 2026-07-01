import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { Competitor } from '../../shared/models/models';

@Component({
  selector: 'app-competencia',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="page-header"><div><h1 class="page-title">Competencia</h1><p class="page-sub">Monitoreo de precios de la competencia</p></div></div>
  <div *ngFor="let c of competitors()" class="card" style="margin-bottom:14px">
    <h3 style="font-size:14px;font-weight:600;margin-bottom:12px">{{ c.name }}</h3>
    <table class="table">
      <thead><tr><th>MODELO</th><th>STORAGE</th><th>PRECIO ELLOS</th></tr></thead>
      <tbody>
        <tr *ngFor="let p of c.prices">
          <td>{{ p.modelName }}</td>
          <td>{{ p.storageGb ? p.storageGb + 'GB' : '—' }}</td>
          <td>u$d {{ p.priceUsd }}</td>
        </tr>
      </tbody>
    </table>
  </div>
  <div *ngIf="competitors().length === 0" class="card empty-state"><div class="empty-state__title">Sin datos de competencia</div></div>
  `,
  styles: [`@use '../../shared/styles/shared';`]
})
export class CompetenciaComponent implements OnInit {
  private api = inject(ApiService);
  competitors = signal<Competitor[]>([]);
  ngOnInit() { /* no endpoint implemented yet — show empty */ }
}
