import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-importacion',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="page-header"><div><h1 class="page-title">Importaciones</h1><p class="page-sub">Órdenes de compra al proveedor exterior + flete</p></div></div>
  <div class="card empty-state">
    <div class="empty-state__icon">🌎</div>
    <div class="empty-state__title">Módulo de importaciones</div>
    <div class="empty-state__sub">Gestioná tus PR (Purchase Requests) y IM (Import Manager) desde acá.</div>
  </div>
  `,
  styles: [`@use '../../shared/styles/shared';`]
})
export class ImportacionComponent {}
