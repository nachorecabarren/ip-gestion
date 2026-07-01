import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import {
  PagedResult,
  DashboardKpis,
  QuickSale,
  Entity,
  CatalogModel,
  CatalogAccessory,
  CatalogLocation,
  StockItem,
  StockBulk,
  Sale,
  Purchase,
  Reservation,
  ImportOrder,
  Caja,
  CashMovement,
  ServiceClientJob,
  EntityBalance,
  RetentionRule,
  RetentionTouchpoint,
  Competitor,
  EntityType,
  StockStatus,
  StockCondition,
  SaleCategory,
  SaleOrigin,
  ReservationStatus,
  ServiceJobStatus,
  TradeInQuote,
  TradeInQuoteRequest,
  CalendarEvent,
} from "../../shared/models/models";

@Injectable({ providedIn: "root" })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // ─── DASHBOARD ───────────────────────────────────────────
  getDashboardKpis(periodo = "month"): Observable<DashboardKpis> {
    return this.http.get<DashboardKpis>(`${this.base}/dashboard/kpis`, {
      params: { periodo },
    });
  }
  getRecentSales(count = 10): Observable<QuickSale[]> {
    return this.http.get<QuickSale[]>(`${this.base}/dashboard/recent-sales`, {
      params: { count },
    });
  }
  getTcBlue(): Observable<{ rate: number }> {
    return this.http.get<{ rate: number }>(`${this.base}/tc-blue`);
  }

  // ─── ENTITIES ────────────────────────────────────────────
  getEntities(
    type?: EntityType,
    search?: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<Entity>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (type) params = params.set("type", type);
    if (search) params = params.set("search", search);
    return this.http.get<PagedResult<Entity>>(`${this.base}/entidades`, {
      params,
    });
  }
  getEntity(id: string): Observable<Entity> {
    return this.http.get<Entity>(`${this.base}/entidades/${id}`);
  }
  createEntity(dto: any): Observable<Entity> {
    return this.http.post<Entity>(`${this.base}/entidades`, dto);
  }
  updateEntity(id: string, dto: any): Observable<Entity> {
    return this.http.put<Entity>(`${this.base}/entidades/${id}`, dto);
  }

  // ─── CATALOGS ────────────────────────────────────────────
  getCatalogModels(): Observable<CatalogModel[]> {
    return this.http.get<CatalogModel[]>(`${this.base}/catalogos/modelos`);
  }
  getCatalogAccessories(): Observable<CatalogAccessory[]> {
    return this.http.get<CatalogAccessory[]>(
      `${this.base}/catalogos/accesorios`,
    );
  }
  getCatalogLocations(): Observable<CatalogLocation[]> {
    return this.http.get<CatalogLocation[]>(
      `${this.base}/catalogos/ubicaciones`,
    );
  }
  createCatalogModel(name: string, idType = "IMEI"): Observable<CatalogModel> {
    return this.http.post<CatalogModel>(`${this.base}/catalogos/modelos`, {
      name,
      idType,
    });
  }
  createCatalogAccessory(name: string): Observable<CatalogAccessory> {
    return this.http.post<CatalogAccessory>(
      `${this.base}/catalogos/accesorios`,
      { name },
    );
  }
  createCatalogLocation(name: string): Observable<CatalogLocation> {
    return this.http.post<CatalogLocation>(
      `${this.base}/catalogos/ubicaciones`,
      { name },
    );
  }

  // ─── STOCK ───────────────────────────────────────────────
  getStockItems(
    status?: StockStatus,
    condition?: StockCondition,
    search?: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<StockItem>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (status) params = params.set("status", status);
    if (condition) params = params.set("condition", condition);
    if (search) params = params.set("search", search);
    return this.http.get<PagedResult<StockItem>>(`${this.base}/stock/items`, {
      params,
    });
  }
  getStockItemByBarcode(barcode: string): Observable<StockItem> {
    return this.http.get<StockItem>(
      `${this.base}/stock/items/barcode/${barcode}`,
    );
  }
  createStockItem(dto: any): Observable<StockItem> {
    return this.http.post<StockItem>(`${this.base}/stock/items`, dto);
  }
  updateStockItem(id: string, dto: any): Observable<StockItem> {
    return this.http.put<StockItem>(`${this.base}/stock/items/${id}`, dto);
  }
  voidStockItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/stock/items/${id}`);
  }
  getStockBulk(): Observable<StockBulk[]> {
    return this.http.get<StockBulk[]>(`${this.base}/stock/bulk`);
  }
  getTradeInQuote(dto: TradeInQuoteRequest): Observable<TradeInQuote> {
    return this.http.post<TradeInQuote>(
      `${this.base}/stock/tradein/quote`,
      dto,
    );
  }
  bulkUpdatePrices(itemIds: string[], newPrice: number): Observable<void> {
    return this.http.post<void>(`${this.base}/stock/items/bulk-price`, {
      itemIds,
      newPrice,
    });
  }
  transferStock(itemIds: string[], targetLocationId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/stock/items/transfer`, {
      itemIds,
      targetLocationId,
    });
  }

  // ─── VENTAS ──────────────────────────────────────────────
  getSales(
    category?: SaleCategory,
    origin?: SaleOrigin,
    search?: string,
    from?: string,
    to?: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<Sale>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (category) params = params.set("category", category);
    if (origin) params = params.set("origin", origin);
    if (search) params = params.set("search", search);
    if (from) params = params.set("from", from);
    if (to) params = params.set("to", to);
    return this.http.get<PagedResult<Sale>>(`${this.base}/ventas`, { params });
  }
  getSale(id: string): Observable<Sale> {
    return this.http.get<Sale>(`${this.base}/ventas/${id}`);
  }
  createSale(dto: any): Observable<Sale> {
    return this.http.post<Sale>(`${this.base}/ventas`, dto);
  }
  voidSale(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/ventas/${id}`);
  }

  // ─── COMPRAS ─────────────────────────────────────────────
  getPurchases(page = 1, pageSize = 20): Observable<PagedResult<Purchase>> {
    return this.http.get<PagedResult<Purchase>>(`${this.base}/compras`, {
      params: { page, pageSize },
    });
  }
  createPurchase(dto: any): Observable<Purchase> {
    return this.http.post<Purchase>(`${this.base}/compras`, dto);
  }
  voidPurchase(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/compras/${id}`);
  }

  // ─── RESERVAS ────────────────────────────────────────────
  getReservations(
    status?: ReservationStatus,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<Reservation>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (status) params = params.set("status", status);
    return this.http.get<PagedResult<Reservation>>(`${this.base}/reservas`, {
      params,
    });
  }
  createReservation(dto: any): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/reservas`, dto);
  }
  cancelReservation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/reservas/${id}`);
  }
  convertReservationToSale(reservationId: string, dto: any): Observable<Sale> {
    return this.http.post<Sale>(
      `${this.base}/reservas/${reservationId}/convertir`,
      dto,
    );
  }

  // ─── CAJAS ───────────────────────────────────────────────
  getCajas(): Observable<Caja[]> {
    return this.http.get<Caja[]>(`${this.base}/cajas`);
  }
  getCashMovements(
    cajaId?: string,
    from?: string,
    to?: string,
    page = 1,
    pageSize = 30,
  ): Observable<CashMovement[]> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (cajaId) params = params.set("cajaId", cajaId);
    if (from) params = params.set("from", from);
    if (to) params = params.set("to", to);
    return this.http.get<CashMovement[]>(`${this.base}/cajas/movimientos`, {
      params,
    });
  }
  registerCashMovement(dto: any): Observable<CashMovement> {
    return this.http.post<CashMovement>(`${this.base}/cajas/movimientos`, dto);
  }
  closeDay(date: string): Observable<void> {
    return this.http.post<void>(`${this.base}/cajas/cierre`, { date });
  }

  // ─── SERVICIO TÉCNICO ────────────────────────────────────
  getServiceJobs(
    status?: ServiceJobStatus,
    search?: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<ServiceClientJob>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (status) params = params.set("status", status);
    if (search) params = params.set("search", search);
    return this.http.get<PagedResult<ServiceClientJob>>(
      `${this.base}/servicio-tecnico`,
      { params },
    );
  }
  createServiceJob(dto: any): Observable<ServiceClientJob> {
    return this.http.post<ServiceClientJob>(
      `${this.base}/servicio-tecnico`,
      dto,
    );
  }
  updateServiceJobStatus(
    id: string,
    status: ServiceJobStatus,
  ): Observable<ServiceClientJob> {
    return this.http.put<ServiceClientJob>(
      `${this.base}/servicio-tecnico/${id}/status`,
      { status },
    );
  }
  voidServiceJob(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/servicio-tecnico/${id}`);
  }

  // ─── CUENTAS CORRIENTES ──────────────────────────────────
  getBalances(
    type?: EntityType,
    filter?: string,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<EntityBalance>> {
    let params = new HttpParams().set("page", page).set("pageSize", pageSize);
    if (type) params = params.set("type", type);
    if (filter) params = params.set("filter", filter);
    return this.http.get<PagedResult<EntityBalance>>(
      `${this.base}/cuentas-corrientes`,
      { params },
    );
  }
  recordPayment(dto: any): Observable<void> {
    return this.http.post<void>(`${this.base}/cuentas-corrientes/pago`, dto);
  }

  // ─── RETENCIÓN ───────────────────────────────────────────
  getRetentionRules(): Observable<RetentionRule[]> {
    return this.http.get<RetentionRule[]>(`${this.base}/retencion/reglas`);
  }
  upsertRetentionRule(dto: RetentionRule): Observable<RetentionRule> {
    return this.http.put<RetentionRule>(`${this.base}/retencion/reglas`, dto);
  }
  getTouchpoints(status?: string): Observable<RetentionTouchpoint[]> {
    let params = new HttpParams();
    if (status) params = params.set("status", status);
    return this.http.get<RetentionTouchpoint[]>(
      `${this.base}/retencion/touchpoints`,
      { params },
    );
  }
  // ─── AGENDA ───────────────────────────────────────────

  getCalendarEvents(year: number, month: number): Observable<CalendarEvent[]> {
    return this.http.get<CalendarEvent[]>(`${this.base}/agenda`, {
      params: { year, month },
    });
  }
  createCalendarEvent(dto: any): Observable<CalendarEvent> {
    return this.http.post<CalendarEvent>(`${this.base}/agenda`, dto);
  }
  deleteCalendarEvent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/agenda/${id}`);
  }
}
