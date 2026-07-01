import { Routes } from '@angular/router';
import { MainLayoutComponent } from './core/layout/main-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { ownerGuard } from './core/guards/owner.guard';

export const routes: Routes = [
  // ─── Standalone auth pages (no sidebar) ───────────────────
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'registro',
    loadComponent: () => import('./features/registro/registro.component').then(m => m.RegistroComponent)
  },
  {
    path: 'aceptar-invitacion',
    loadComponent: () => import('./features/aceptar-invitacion/aceptar-invitacion.component').then(m => m.AceptarInvitacionComponent)
  },
  // ─── App shell (requires auth) ────────────────────────────
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'ventas', loadComponent: () => import('./features/ventas/ventas.component').then(m => m.VentasComponent) },
      { path: 'compras', loadComponent: () => import('./features/compras/compras.component').then(m => m.ComprasComponent) },
      { path: 'importacion', loadComponent: () => import('./features/importacion/importacion.component').then(m => m.ImportacionComponent) },
      { path: 'reservas', loadComponent: () => import('./features/reservas/reservas.component').then(m => m.ReservasComponent) },
      { path: 'stock', loadComponent: () => import('./features/stock/stock.component').then(m => m.StockComponent) },
      { path: 'cajas', loadComponent: () => import('./features/cajas/cajas.component').then(m => m.CajasComponent) },
      { path: 'base-datos', loadComponent: () => import('./features/base-datos/base-datos.component').then(m => m.BaseDatosComponent) },
      { path: 'servicio-tecnico', loadComponent: () => import('./features/servicio-tecnico/servicio-tecnico.component').then(m => m.ServicioTecnicoComponent) },
      { path: 'cuentas-corrientes', loadComponent: () => import('./features/cuentas-corrientes/cuentas-corrientes.component').then(m => m.CuentasCorrientesComponent) },
      { path: 'proveedores', loadComponent: () => import('./features/proveedores/proveedores.component').then(m => m.ProveedoresComponent) },
      { path: 'retencion', loadComponent: () => import('./features/retencion/retencion.component').then(m => m.RetencionComponent) },
      { path: 'objeciones', loadComponent: () => import('./features/objeciones/objeciones.component').then(m => m.ObjecionesComponent) },
      { path: 'competencia', loadComponent: () => import('./features/competencia/competencia.component').then(m => m.CompetenciaComponent) },
      { path: 'agenda', loadComponent: () => import('./features/agenda/agenda.component').then(m => m.AgendaComponent) },
      // OWNER-only:
      { path: 'equipo', canActivate: [ownerGuard], loadComponent: () => import('./features/equipo/equipo.component').then(m => m.EquipoComponent) },
      { path: 'configuracion', canActivate: [ownerGuard], loadComponent: () => import('./features/configuracion/configuracion.component').then(m => m.ConfiguracionComponent) },
    ]
  },
  { path: '**', redirectTo: 'login' }
];
