import { Component, OnInit, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { ApiService } from "../../core/services/api.service";
import { DashboardKpis, QuickSale, Reservation, ServiceClientJob } from "../../shared/models/models";

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);

  activeTab = signal<'financiero' | 'operativo'>('financiero');
  periodo = signal<"week" | "month" | "year">("month");
  kpis = signal<DashboardKpis | null>(null);
  recentSales = signal<QuickSale[]>([]);
  reservations = signal<Reservation[]>([]);
  serviceJobs = signal<ServiceClientJob[]>([]);
  loading = signal(true);

  readonly periodoLabels = {
    week: "Esta semana",
    month: "Este mes",
    year: "Este año",
  };

  readonly quickActions = [
    {
      label: "Nueva Venta",
      sublabel: "Facturar iPhone",
      route: "/ventas",
      icon: "cart",
    },
    {
      label: "Nueva Compra",
      sublabel: "Ingresar Stock",
      route: "/compras",
      icon: "box",
    },
    {
      label: "Nueva Reserva",
      sublabel: "Apartar modelo",
      route: "/reservas",
      icon: "bookmark",
    },
    {
      label: "Nuevo Movimiento",
      sublabel: "Ajuste de caja",
      route: "/cajas",
      icon: "dollar",
    },
  ];

  ngOnInit() {
    this.loadData();
    this.api.getReservations('ACTIVE').subscribe(r => this.reservations.set(r.items));
    this.api.getServiceJobs().subscribe(r =>
      this.serviceJobs.set(r.items.filter(j => !['DELIVERED', 'CANCELLED', 'CLOSED'].includes(j.status)))
    );
  }

  loadData() {
    this.loading.set(true);
    this.api.getDashboardKpis(this.periodo()).subscribe({
      next: (kpis) => {
        this.kpis.set(kpis);
        this.loading.set(false);
      },
    });
    this.api.getRecentSales(8).subscribe((s) => this.recentSales.set(s));
  }

  setPeriodo(p: "week" | "month" | "year") {
    this.periodo.set(p);
    this.loadData();
  }

  formatUsd(v: number) {
    return `u$d ${v.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  isPastPickup(d: string) { return new Date(d) < new Date(); }
  isNearPickup(d: string) {
    const ms = new Date(d).getTime() - Date.now();
    return ms >= 0 && ms < 3 * 86_400_000;
  }
  isJobOverdue(j: ServiceClientJob) {
    if (!j.limitDate || ['DELIVERED', 'CANCELLED', 'CLOSED'].includes(j.status)) return false;
    return new Date(j.limitDate) < new Date();
  }
  getServiceStatusLabel(s: string): string {
    return ({ OPEN: 'Abierto', IN_REPAIR: 'En reparación', READY_FOR_DELIVERY: 'Listo para entrega', DELIVERED: 'Entregado', CANCELLED: 'Cancelado', CLOSED: 'Cerrado' } as Record<string,string>)[s] ?? s;
  }
  getServiceStatusClass(s: string): string {
    return ({ OPEN: 'badge--blue', IN_REPAIR: 'badge--amber', READY_FOR_DELIVERY: 'badge--green', DELIVERED: 'badge--gray', CANCELLED: 'badge--red' } as Record<string,string>)[s] ?? 'badge--gray';
  }
}
