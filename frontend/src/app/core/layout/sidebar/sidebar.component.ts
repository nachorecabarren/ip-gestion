import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

interface NavItem { label: string; route: string; icon: string; ownerOnly?: boolean; }
interface NavGroup { group: string; items: NavItem[]; }

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  private api = inject(ApiService);
  auth = inject(AuthService);
  tcBlue = signal(1520);

  constructor() {
    this.api.getTcBlue().subscribe(r => this.tcBlue.set(r.rate));
  }

  readonly nav: NavGroup[] = [
    {
      group: 'PRINCIPAL',
      items: [
        { label: 'Dashboard', route: '/dashboard', icon: 'grid' },
        { label: 'Ventas / Canjes', route: '/ventas', icon: 'cart' },
        { label: 'Compras', route: '/compras', icon: 'box' },
        // { label: 'Importaciones', route: '/importacion', icon: 'globe' },
        { label: 'Reservas', route: '/reservas', icon: 'bookmark' },
        { label: 'Stock', route: '/stock', icon: 'package' },
      ]
    },
    {
      group: 'ADMINISTRACIÓN',
      items: [
        { label: 'Cajas', route: '/cajas', icon: 'wallet' },
        { label: 'Base de Datos', route: '/base-datos', icon: 'database' },
        { label: 'Serv. Técnico', route: '/servicio-tecnico', icon: 'tool' },
        { label: 'Cuentas Ctes.', route: '/cuentas-corrientes', icon: 'users' },
        { label: 'Proveedores', route: '/proveedores', icon: 'truck' },
      ]
    },
    {
      group: 'MARKETING',
      items: [
        { label: 'Retención', route: '/retencion', icon: 'heart' },
        // { label: 'Objeciones', route: '/objeciones', icon: 'message' },
        // { label: 'Competencia', route: '/competencia', icon: 'zap' },
        { label: 'Agenda', route: '/agenda', icon: 'calendar' },
      ]
    },
    {
      group: 'SISTEMA',
      items: [
        { label: 'Mi Equipo', route: '/equipo', icon: 'users', ownerOnly: true },
        { label: 'Configuración', route: '/configuracion', icon: 'settings', ownerOnly: true },
      ]
    }
  ];

  /** Items visible for the current role (owner-only links hidden for employees). */
  visibleItems(group: NavGroup): NavItem[] {
    const owner = this.auth.isOwner();
    return group.items.filter(i => !i.ownerOnly || owner);
  }

  roleLabel(): string {
    return this.auth.isOwner() ? 'Dueño' : 'Empleado';
  }
}
