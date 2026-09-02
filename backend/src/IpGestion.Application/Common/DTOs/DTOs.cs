using IpGestion.Domain.Enums;

namespace IpGestion.Application.Common.DTOs;

// ─── SHARED ────────────────────────────────────────────────
public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);

public record PaymentDto(
    PaymentMethod Method,
    Currency Currency,
    decimal Amount,
    decimal ExchangeRateUsd
);

// ─── DASHBOARD ─────────────────────────────────────────────
public record DashboardKpisDto(
    decimal FacturacionPeriodo,
    decimal MargenBruto,
    decimal GananciaNeta,
    int CantidadVentas,
    int StockDisponible,
    int ReservasActivas,
    decimal GastosClosers,
    decimal GastosOperativos,
    decimal RegalosAccesorios,
    int VentasConCanje,
    decimal PorcentajeCanje,
    string Periodo
);

public record QuickSaleDto(DateTime Date, string Item, string Client, decimal Amount, decimal Margin);

public record DashboardTrendPointDto(string Period, decimal FacturacionUsd, int CantidadVentas, int VentasConCanje);

// ─── ENTITY (Clientes, Proveedores, etc.) ──────────────────
public record EntityDto(
    Guid Id,
    EntityType Type,
    string Name,
    string? Phone,
    string? Email,
    string? Instagram,
    string? DocumentNumber,
    string? AddressCity,
    bool IsActive,
    decimal? BalanceUsd
);

public record CreateEntityDto(
    EntityType Type,
    string Name,
    string? Phone,
    string? Email,
    string? Instagram,
    string? DocumentNumber,
    string? AddressCity,
    string? AddressStreet,
    string? ShippingBranch,
    string? ShippingPostalCode,
    string? ShippingCity,
    string? ShippingProvince,
    string? ShippingNotes,
    string? PreferredTransport
);

public record UpdateEntityDto(
    string Name,
    string? Phone,
    string? Email,
    string? Instagram,
    string? DocumentNumber,
    string? AddressCity,
    string? AddressStreet,
    bool IsActive
);

// ─── CATALOG ───────────────────────────────────────────────
public record CatalogModelDto(Guid Id, string Name, string IdType, bool RequiresStorage, bool RequiresColor);
public record CatalogAccessoryDto(Guid Id, string Name, bool RequiresModel, bool RequiresColor);
public record CatalogLocationDto(Guid Id, string Name);

// ─── STOCK ─────────────────────────────────────────────────
public record StockItemDto(
    Guid Id,
    Guid ModelId,
    string ModelName,
    string? InternalCode,
    string? ImeiSerial,
    string? Color,
    int? StorageGb,
    StockCondition Condition,
    string? ConditionGrade,
    int? BatteryPct,
    decimal CostUsd,
    decimal SuggestedPriceUsd,
    decimal? WholesalePriceUsd,
    StockStatus Status,
    Guid? LocationId,
    string? LocationName,
    string? Notes,
    DateTime CreatedAt
);

public record StockBulkDto(
    Guid Id,
    string AccessoryName,
    string? Color,
    int Quantity,
    int LowStockThreshold,
    decimal CostUsd,
    decimal SuggestedPriceUsd,
    string? LocationName
);

public record CreateStockItemDto(
    Guid ModelId,
    string? ImeiSerial,
    string? Color,
    int? StorageGb,
    StockCondition Condition,
    int? BatteryPct,
    decimal CostUsd,
    decimal SuggestedPriceUsd,
    decimal? WholesalePriceUsd,
    Guid? LocationId,
    string? Notes
);

public record UpdateStockItemDto(
    string? Color,
    int? StorageGb,
    StockCondition Condition,
    string? ConditionGrade,
    int? BatteryPct,
    decimal SuggestedPriceUsd,
    decimal? WholesalePriceUsd,
    Guid? LocationId,
    string? Notes
);

// ─── VENTAS ────────────────────────────────────────────────
public record SaleDto(
    Guid Id,
    string? ClientName,
    string? ClientPhone,
    SaleCategory Category,
    SaleOrigin Origin,
    decimal TotalUsd,
    decimal MarginUsd,
    int WarrantyDays,
    SaleStatus Status,
    string? Notes,
    DateTime SaleDate,
    List<SaleItemDto> Items,
    List<PaymentDto> Payments,
    string? SoldBy,
    bool HasTradeIn,
    decimal? TradeInValueUsd
);

public record SaleItemDto(
    Guid Id,
    ItemKind Type,
    string ItemName,
    int Quantity,
    decimal PriceUsd,
    string? ImeiSerial
);

public record CreateSaleDto(
    DateTime SaleDate,
    Guid? EntityId,
    string? RetailClientName,
    string? RetailClientPhone,
    string? RetailClientInstagram,
    SaleCategory SaleCategory,
    SaleOrigin Origin,
    decimal TotalUsd,
    int WarrantyDays,
    string? Notes,
    List<CreateSaleItemDto> Items,
    List<PaymentDto> Payments,
    List<Guid> CloserIds,
    TradeInDto? TradeIn,
    string? InvoiceEmail = null,
    bool SendInvoiceEmail = false
);

public record CreateSaleItemDto(
    ItemKind Type,
    Guid? StockItemId,
    Guid? StockBulkId,
    int Quantity,
    decimal PriceUsd
);

public record TradeInDto(
    Guid ModelId,
    string? ImeiSerial,
    string? Color,
    int StorageGb,
    int BatteryPct,
    StockCondition Condition,
    decimal ValueUsd,
    decimal SuggestedPriceUsd
);

// ─── COMPRAS ───────────────────────────────────────────────
public record PurchaseDto(
    Guid Id,
    string? ProviderName,
    Guid? ProviderId,
    DateTime PurchaseDate,
    decimal TotalUsd,
    PurchaseType Type,
    PurchaseStatus Status,
    string? Notes,
    List<StockItemDto> StockItems,
    List<PurchaseBulkItemDto> BulkItems
);

public record PurchaseBulkItemDto(
    Guid Id,
    Guid AccessoryId,
    string AccessoryName,
    string? Color,
    int Quantity,
    decimal CostUsd,
    decimal SuggestedPriceUsd
);

public record CreatePurchaseDto(
    Guid? ProviderId,
    DateTime PurchaseDate,
    PurchaseType Type,
    string? Notes,
    List<CreateStockItemDto> DeviceItems,
    List<CreateBulkItemDto> BulkItems,
    List<PaymentDto>? Payments
);

public record CreateBulkItemDto(
    Guid AccessoryId,
    Guid? ModelId,
    string? Color,
    int Quantity,
    decimal CostUsd,
    decimal SuggestedPriceUsd
);

// Editar una compra existente — solo corrige valores de líneas ya creadas
// (no agrega/quita ítems); requiere owner o admin, validado en el controller.
public record UpdatePurchaseDto(
    Guid? ProviderId,
    DateTime PurchaseDate,
    string? Notes,
    List<UpdatePurchaseDeviceItemDto> DeviceItems,
    List<UpdatePurchaseBulkItemDto> BulkItems
);

public record UpdatePurchaseDeviceItemDto(
    Guid Id,
    Guid ModelId,
    string? ImeiSerial,
    string? Color,
    int? StorageGb,
    StockCondition Condition,
    int? BatteryPct,
    decimal CostUsd,
    decimal SuggestedPriceUsd,
    decimal? WholesalePriceUsd
);

public record UpdatePurchaseBulkItemDto(
    Guid Id,
    Guid AccessoryId,
    string? Color,
    int Quantity,
    decimal CostUsd,
    decimal SuggestedPriceUsd
);

// ─── RESERVAS ──────────────────────────────────────────────
public record ReservationDto(
    Guid Id,
    Guid? EntityId,
    Guid? StockItemId,
    string? ClientName,
    string? ClientPhone,
    string? ItemName,
    SaleCategory Category,
    DateTime PickupDate,
    ReservationStatus Status,
    decimal DepositAmountUsd,
    decimal AgreedPriceUsd,
    string? Notes,
    DateTime CreatedAt
);

public record CreateReservationDto(
    Guid? EntityId,
    string? RetailClientName,
    string? RetailClientPhone,
    string? RetailClientInstagram,
    Guid? StockItemId,
    Guid? StockBulkId,
    int? BulkQuantity,
    SaleCategory SaleCategory,
    DateTime PickupDate,
    decimal AgreedPriceUsd,
    decimal DepositAmountUsd,
    PaymentMethod? DepositMethod,
    string? Notes
);

// ─── IMPORTACION ───────────────────────────────────────────
public record ImportOrderDto(
    Guid Id,
    string? ProviderName,
    string? CourierName,
    ImportOrderStatus Status,
    decimal TotalUsd,
    string? Notes,
    DateTime CreatedAt,
    List<ImportOrderItemDto> Items
);

public record ImportOrderItemDto(
    Guid Id,
    string? ModelName,
    string? AccessoryName,
    StockItemConditionDetail Condition,
    int Quantity,
    decimal CostProvUsd,
    int WeightGrams,
    decimal CourierUsdPerKg,
    decimal FleteUsd,
    decimal CostoFinalUsd,
    string? Notes
);

public record CreateImportOrderDto(
    Guid? ProviderId,
    Guid? CourierId,
    string? Notes,
    List<CreateImportOrderItemDto> Items
);

public record CreateImportOrderItemDto(
    Guid? ModelId,
    Guid? AccessoryId,
    StockItemConditionDetail Condition,
    int Quantity,
    decimal CostProvUsd,
    int WeightGrams,
    decimal CourierUsdPerKg,
    string? Notes,
    string? DetailObs
);

public record ReceiveImportItemDto(
    Guid ItemId,
    string? ImeiSerial,
    string? Color,
    int? StorageGb,
    int? BatteryPct
);

// ─── CAJAS ─────────────────────────────────────────────────
public record CajaDto(
    Guid Id,
    string Name,
    bool IsDefault,
    bool IsActive,
    decimal BalanceUsdCash,
    decimal BalanceUsdt,
    decimal BalanceArsCash,
    decimal BalanceArsTr,
    decimal BalanceMercadoPago
);

public record CashMovementDto(
    Guid Id,
    string CajaName,
    CashMovementType Type,
    PaymentMethod Method,
    decimal Amount,
    decimal AmountUsd,
    Currency Currency,
    string? Detail,
    string? Category,
    DateTime CreatedAt
);

public record CreateCashMovementDto(
    Guid CajaId,
    CashMovementType Type,
    PaymentMethod Method,
    Currency Currency,
    decimal Amount,
    decimal ExchangeRateUsd,
    Guid? CategoryId,
    string? Detail
);

// Vista previa (sin persistir) de lo que un cierre calcularía ahora mismo.
public record CashClosingPreviewDto(
    Guid CajaId,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    decimal ExpectedUsdCash,
    decimal ExpectedArsCash,
    decimal ExpectedUsdt,
    decimal ExpectedArsTr,
    decimal ExpectedMercadoPago,
    decimal IngresosUsd,
    decimal EgresosUsd
);

public record CloseCajaDto(
    Guid CajaId,
    decimal CountedUsdCash,
    decimal CountedArsCash,
    string? Notes
);

public record CashClosingDto(
    Guid Id,
    Guid CajaId,
    string CajaName,
    DateTime PeriodFrom,
    DateTime PeriodTo,
    decimal ExpectedUsdCash,
    decimal CountedUsdCash,
    decimal DiffUsdCash,
    decimal ExpectedArsCash,
    decimal CountedArsCash,
    decimal DiffArsCash,
    decimal ExpectedUsdt,
    decimal ExpectedArsTr,
    decimal ExpectedMercadoPago,
    decimal IngresosUsd,
    decimal EgresosUsd,
    string UserName,
    string? Notes,
    DateTime CreatedAt
);

// ─── SERVICIO TÉCNICO ──────────────────────────────────────
public record ServiceClientJobDto(
    Guid Id,
    string SvCode,
    string ClientName,
    string? ClientPhone,
    string? ClientEmail,
    string? DeviceModel,
    string? ImeiSerial,
    string IssueDescription,
    string? TechnicianName,
    decimal PriceToClientUsd,
    decimal TechnicianCostUsd,
    decimal DepositAmount,
    ServiceJobStatus Status,
    DateOnly? LimitDate,
    DateTime CreatedAt
);

public record CreateServiceClientJobDto(
    string RetailClientName,
    string? RetailClientPhone,
    string? RetailClientEmail,
    string? DeviceModel,
    string? ImeiSerial,
    string IssueDescription,
    Guid? TechnicianId,
    decimal PriceToClientUsd,
    decimal TechnicianCostUsd,
    PaymentMethod? DepositMethod,
    decimal DepositAmount,
    DateOnly? LimitDate
);

public record UpdateServiceJobStatusDto(ServiceJobStatus Status);

// ─── CUENTAS CORRIENTES ────────────────────────────────────
public record EntityBalanceDto(
    Guid EntityId,
    string EntityName,
    string? Phone,
    EntityType Type,
    decimal BalanceUsd
);

public record RecordDebtPaymentDto(
    Guid EntityId,
    decimal AmountUsd,
    PaymentMethod Method,
    Currency Currency,
    decimal ExchangeRateUsd,
    string? Notes
);

// ─── RETENCIÓN ─────────────────────────────────────────────
public record RetentionRuleDto(
    Guid Id,
    string RuleType,
    int DaysAfterSale,
    string MessageTemplate,
    bool IsActive
);

public record RetentionTouchpointDto(
    Guid SaleId,
    string ClientName,
    string? ClientPhone,
    string RuleType,
    string MessageTemplate,
    DateTime TriggerDate,
    string Status
);

public record RetentionTouchpointsPageDto(
    IEnumerable<RetentionTouchpointDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalAll,
    int ParaHoyCount,
    int VencidoCount,
    int PendienteCount
);

// ─── OBJECIONES ────────────────────────────────────────────
public record ObjectionDto(
    Guid Id,
    string RawText,
    string? SuggestedResponse,
    string Source,
    DateTime CreatedAt
);

public record CreateObjectionDto(string RawText, Guid? SaleId);

// ─── COMPETENCIA ───────────────────────────────────────────
public record CompetitorDto(
    Guid Id,
    string Name,
    bool IsActive,
    List<CompetitorPriceDto> Prices
);

public record CompetitorPriceDto(
    string ModelName,
    int? StorageGb,
    decimal PriceUsd,
    decimal? OurPriceUsd,
    decimal? DiffUsd
);

// ─── AGENDA ────────────────────────────────────────────────
public record CalendarEventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string Type
);

public record CreateCalendarEventDto(
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string Type
);

// ─── TRADE-IN COTIZADOR ────────────────────────────────────
public record TradeInQuoteRequestDto(string ModelName, int StorageGb, int BatteryPct);
public record TradeInQuoteDto(decimal BaseValueUsd, decimal AdjustedValueUsd, string Notes);

public record TradeInHistoryDto(
    Guid Id,
    DateTime SaleDate,
    string ModelName,
    string? ImeiSerial,
    int StorageGb,
    int BatteryPct,
    decimal ValueUsd,
    string ClientName,
    Guid SaleId,
    Guid? StockItemId
);

// ─── AUTH ──────────────────────────────────────────────────
public record LoginRequestDto(string Email, string Password);

public record RegisterRequestDto(
    string BusinessName,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerDisplayName
);

// Carries everything needed to build the JWT and the /me response
public record AuthUserDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    string DisplayName,
    string Role
);

// ─── INVITATIONS ───────────────────────────────────────────
public record CreateInvitationDto(string Email);

public record InvitationLinkDto(
    Guid Id,
    string Email,
    string Token,
    string AcceptUrl,
    DateTime ExpiresAt
);

public record InvitationInfoDto(string Email, string BusinessName);

public record AcceptInvitationDto(string Token, string DisplayName, string Password);

public record PendingInvitationDto(
    Guid Id,
    string Email,
    string Token,
    DateTime CreatedAt,
    DateTime ExpiresAt
);

// ─── TEAM / USERS ──────────────────────────────────────────
public record TenantUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateRoleDto(string Role);

// ─── AUDITORÍA ─────────────────────────────────────────────
public record AuditLogDto(
    Guid Id,
    string UserName,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? Details,
    DateTime CreatedAt
);
