import { Component, OnInit, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { ApiService } from "../../core/services/api.service";
import { DashboardKpis, DashboardTrendPoint, QuickSale, Reservation, ServiceClientJob, StockBulk } from "../../shared/models/models";
import { BarChartComponent } from "../../shared/components/bar-chart/bar-chart.component";

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [CommonModule, RouterModule, BarChartComponent],
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
  trend = signal<DashboardTrendPoint[]>([]);
  loading = signal(true);
  lowStockItems = signal<StockBulk[]>([]);
  lowStockAlertDismissed = signal(false);

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
      queryParams: { nueva: "1" },
      icon: "cart",
    },
    {
      label: "Nueva Compra",
      sublabel: "Ingresar Stock",
      route: "/compras",
      queryParams: { nueva: "1" },
      icon: "box",
    },
    {
      label: "Nueva Reserva",
      sublabel: "Apartar modelo",
      route: "/reservas",
      queryParams: { nueva: "1" },
      icon: "bookmark",
    },
    {
      label: "Nuevo Movimiento",
      sublabel: "Ajuste de caja",
      route: "/cajas",
      queryParams: { nueva: "1" },
      icon: "dollar",
    },
  ];

  ngOnInit() {
    this.loadData();
    this.api.getReservations('ACTIVE').subscribe(r => this.reservations.set(r.items));
    this.api.getServiceJobs().subscribe(r =>
      this.serviceJobs.set(r.items.filter(j => !['DELIVERED', 'CANCELLED', 'CLOSED'].includes(j.status)))
    );
    this.api.getDashboardTrend(6).subscribe(t => this.trend.set(t));
    this.api.getStockBulk().subscribe(items =>
      this.lowStockItems.set(items.filter(b => b.quantity <= b.lowStockThreshold))
    );
  }

  dismissLowStockAlert() {
    this.lowStockAlertDismissed.set(true);
  }

  get lowStockNamesPreview(): string {
    const names = this.lowStockItems().slice(0, 4).map(i => i.accessoryName).join(', ');
    return this.lowStockItems().length > 4 ? `${names}…` : names;
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

  get trendFacturacion() {
    return this.trend().map(t => ({ label: t.period, value: t.facturacionUsd }));
  }
  get trendCanjes() {
    return this.trend().map(t => ({ label: t.period, value: t.ventasConCanje }));
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
