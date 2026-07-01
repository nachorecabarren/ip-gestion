// ─── ENUMS ──────────────────────────────────────────────────
export type TenantPlan = 'STARTER' | 'ADVANCED' | 'PRO';
export type EntityType = 'CLIENT' | 'PROVIDER' | 'EMPLOYEE' | 'TECHNICIAN' | 'COURIER';
export type StockCondition = 'NEW' | 'USED' | 'REFURBISHED' | 'A_PLUS' | 'A' | 'B';
export type StockStatus = 'AVAILABLE' | 'RESERVED' | 'SOLD' | 'IN_SERVICE' | 'IN_REPAIR' | 'RETURNED' | 'VOIDED';
export type SaleCategory = 'RETAIL' | 'WHOLESALE';
export type SaleOrigin = 'DIRECT' | 'RESERVATION';
export type SaleStatus = 'COMPLETED' | 'VOIDED' | 'CANCELLED' | 'PENDING';
export type PurchaseStatus = 'PENDING' | 'ACTIVE' | 'PARTIAL' | 'DELIVERED' | 'CANCELLED';
export type PurchaseType = 'DEVICE' | 'ACCESSORY';
export type ReservationStatus = 'ACTIVE' | 'SOLD' | 'CANCELLED';
export type ServiceJobStatus = 'OPEN' | 'IN_REPAIR' | 'READY_FOR_DELIVERY' | 'DELIVERED' | 'CLOSED' | 'CANCELLED';
export type PaymentMethod = 'USD_CASH' | 'USDT' | 'ARS_CASH' | 'ARS_TR' | 'MERCADOPAGO';
export type Currency = 'USD' | 'ARS';
export type CashMovementType = 'INCOME' | 'EXPENSE' | 'SALE' | 'PURCHASE';
export type ItemKind = 'EQUIPMENT' | 'ACCESSORY';
export type ImportOrderStatus = 'PENDING' | 'IN_TRANSIT' | 'RECEIVED' | 'CANCELLED';

// ─── INTERFACES ─────────────────────────────────────────────
export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number; }

export interface Payment { method: PaymentMethod; currency: Currency; amount: number; exchangeRateUsd: number; }

export interface DashboardKpis {
  facturacionPeriodo: number; margenBruto: number; gananciaNeta: number;
  cantidadVentas: number; stockDisponible: number; reservasActivas: number;
  gastosClosers: number; gastosOperativos: number; regalosAccesorios: number; periodo: string;
}

export interface QuickSale { date: string; item: string; client: string; amount: number; margin: number; }

export interface Entity {
  id: string; type: EntityType; name: string; phone?: string; email?: string;
  instagram?: string; documentNumber?: string; addressCity?: string; isActive: boolean; balanceUsd?: number;
}

export interface CatalogModel { id: string; name: string; idType: string; requiresStorage: boolean; requiresColor: boolean; }
export interface CatalogAccessory { id: string; name: string; requiresModel: boolean; requiresColor: boolean; }
export interface CatalogLocation { id: string; name: string; }

export interface StockItem {
  id: string; modelName: string; internalCode?: string; imeiSerial?: string; color?: string;
  storageGb?: number; condition: StockCondition; conditionGrade?: string; batteryPct?: number;
  costUsd: number; suggestedPriceUsd: number; wholesalePriceUsd?: number;
  status: StockStatus; locationName?: string; notes?: string; createdAt: string;
}

export interface StockBulk {
  id: string; accessoryName: string; color?: string; quantity: number; lowStockThreshold: number;
  costUsd: number; suggestedPriceUsd: number; locationName?: string;
}

export interface SaleItem { id: string; type: ItemKind; itemName: string; quantity: number; priceUsd: number; }

export interface Sale {
  id: string; clientName?: string; clientPhone?: string; category: SaleCategory; origin: SaleOrigin;
  totalUsd: number; marginUsd: number; warrantyDays: number; status: SaleStatus;
  notes?: string; saleDate: string; items: SaleItem[]; payments: Payment[]; soldBy?: string;
}

export interface Purchase {
  id: string; providerName?: string; purchaseDate: string; totalUsd: number;
  type: PurchaseType; status: PurchaseStatus; notes?: string; stockItems: StockItem[];
}

export interface Reservation {
  id: string; clientName?: string; clientPhone?: string; itemName?: string; category: SaleCategory;
  pickupDate: string; status: ReservationStatus; depositAmountUsd: number; agreedPriceUsd: number;
  notes?: string; createdAt: string;
}

export interface ImportOrderItem {
  id: string; modelName?: string; accessoryName?: string; condition: string;
  quantity: number; costProvUsd: number; weightGrams: number; courierUsdPerKg: number;
  fleteUsd: number; costoFinalUsd: number; notes?: string;
}

export interface ImportOrder {
  id: string; providerName?: string; courierName?: string; status: ImportOrderStatus;
  totalUsd: number; notes?: string; createdAt: string; items: ImportOrderItem[];
}

export interface Caja {
  id: string; name: string; isDefault: boolean; isActive: boolean;
  balanceUsdCash: number; balanceUsdt: number; balanceArsCash: number; balanceArsTr: number;
}

export interface CashMovement {
  id: string; cajaName: string; type: CashMovementType; method: PaymentMethod;
  amount: number; amountUsd: number; currency: Currency; detail?: string; category?: string; createdAt: string;
}

export interface ServiceClientJob {
  id: string; svCode: string; clientName: string; clientPhone?: string; deviceModel?: string;
  imeiSerial?: string; issueDescription: string; technicianName?: string;
  priceToClientUsd: number; technicianCostUsd: number; depositAmount: number;
  status: ServiceJobStatus; limitDate?: string; createdAt: string;
}

export interface EntityBalance { entityId: string; entityName: string; phone?: string; type: EntityType; balanceUsd: number; }

export interface RetentionRule { id: string; ruleType: string; daysAfterSale: number; messageTemplate: string; isActive: boolean; }
export interface RetentionTouchpoint { saleId: string; clientName: string; clientPhone?: string; ruleType: string; messageTemplate: string; triggerDate: string; status: string; }

export interface Objection { id: string; rawText: string; suggestedResponse?: string; source: string; createdAt: string; }
export interface CompetitorPrice { modelName: string; storageGb?: number; priceUsd: number; ourPriceUsd?: number; diffUsd?: number; }
export interface Competitor { id: string; name: string; isActive: boolean; prices: CompetitorPrice[]; }
export interface CalendarEvent { id: string; title: string; description?: string; startTime: string; endTime: string; type: string; }

// ─── TRADE-IN ───────────────────────────────────────────────
export interface TradeInQuoteRequest { modelName: string; storageGb: number; batteryPct: number; }
export interface TradeInQuote { baseValueUsd: number; adjustedValueUsd: number; notes: string; }

// ─── CREATE DTOs (usados en api.service.ts) ─────────────────
export interface CreateSaleDto { [key: string]: any; }
export interface CreatePurchaseDto { [key: string]: any; }
export interface CreateReservationDto { [key: string]: any; }
export interface CreateImportOrderDto { [key: string]: any; }
export interface CreateServiceClientJobDto { [key: string]: any; }
export interface CreateCashMovementDto { [key: string]: any; }
export interface CreateEntityDto { [key: string]: any; }
export interface UpdateEntityDto { [key: string]: any; }
export interface UpdateStockItemDto { [key: string]: any; }
export interface RecordDebtPaymentDto { [key: string]: any; }