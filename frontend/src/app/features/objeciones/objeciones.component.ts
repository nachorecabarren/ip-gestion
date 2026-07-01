import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-objeciones',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="page-header"><div><h1 class="page-title">Banco de Objeciones</h1><p class="page-sub">Respuestas a objeciones comunes de ventas</p></div></div>
  <div class="card empty-state">
    <div class="empty-state__icon">💬</div>
    <div class="empty-state__title">Sin objeciones registradas</div>
    <div class="empty-state__sub">Registrá objeciones de clientes para mejorar tu discurso de ventas.</div>
  </div>
  `,
  styles: [`@use '../../shared/styles/shared';`]
})
export class ObjecionesComponent {}
