import { Component, ElementRef, HostListener, OnDestroy, AfterViewInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent implements AfterViewInit, OnDestroy {
  private el = inject(ElementRef<HTMLElement>);

  scrolled = signal(false);
  menuOpen = signal(false);

  /** Local preview only — flips the demo card's own colors, independent of the real app theme. */
  darkPreview = signal(false);

  /** Drives which step is highlighted in the "add to home screen" phone mockups. */
  iosStep = signal<1 | 2 | 3>(1);
  androidStep = signal<1 | 2 | 3>(1);

  private readonly whatsappNumber = '5493442507430';
  readonly whatsappUrl = `https://wa.me/${this.whatsappNumber}?text=${
    encodeURIComponent('Hola! Quiero conocer iP Gestión para mi local.')
  }`;

  private observer?: IntersectionObserver;

  readonly modules = [
    { icon: 'cart', title: 'Ventas y Canjes', desc: 'Facturá equipos minoristas o mayoristas y calculá el margen al instante, tomando trade-ins como parte de pago.' },
    { icon: 'box', title: 'Compras', desc: 'Registrá el ingreso de equipos y accesorios con su costo, proveedor y forma de pago.' },
    { icon: 'package', title: 'Stock por IMEI', desc: 'Escaneá el IMEI con la cámara en vez de tipearlo y accedé al instante al historial completo de ese equipo.' },
    { icon: 'wallet', title: 'Cajas por moneda', desc: 'Efectivo USD, ARS, USDT y transferencias, cada una con sus propios movimientos y saldo.' },
    { icon: 'tool', title: 'Servicio Técnico', desc: 'Órdenes de reparación con cliente, falla, técnico asignado, seña y fecha de entrega.' },
    { icon: 'bookmark', title: 'Reservas', desc: 'Apartá un equipo con seña y fecha de retiro sin sacarlo del stock disponible.' },
    { icon: 'users', title: 'Cuentas Corrientes', desc: 'Saldos a cobrar y a pagar con clientes y proveedores, siempre al día.' },
    { icon: 'truck', title: 'Proveedores', desc: 'Un registro central de tus proveedores y las compras que les hiciste.' },
    { icon: 'heart', title: 'Retención', desc: 'Identificá clientes para volver a contactar y no perder ventas futuras.' },
    { icon: 'calendar', title: 'Agenda', desc: 'Turnos, retiros y seguimientos organizados por fecha.' },
    { icon: 'grid', title: 'Dashboard financiero', desc: 'Ventas, márgenes, gastos y tendencias mensuales en un solo panel.' },
    { icon: 'database', title: 'Base de Datos', desc: 'Clientes, técnicos y contactos centralizados para todo el negocio.' },
  ];

  ngAfterViewInit(): void {
    const targets = this.el.nativeElement.querySelectorAll('.reveal');
    this.observer = new IntersectionObserver((entries) => {
      for (const entry of entries) {
        if (entry.isIntersecting) {
          entry.target.classList.add('reveal--visible');
          this.observer?.unobserve(entry.target);
        }
      }
    }, { threshold: 0.15, rootMargin: '0px 0px -40px 0px' });

    targets.forEach((t: Element) => this.observer?.observe(t));
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 8);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
