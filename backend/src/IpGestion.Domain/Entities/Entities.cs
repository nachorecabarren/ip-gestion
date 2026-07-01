using IpGestion.Domain.Enums;

namespace IpGestion.Domain.Entities;

// ─── TENANT ────────────────────────────────────────────────
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "AR";
    public string? LogoUrl { get; set; }
    public TenantPlan Plan { get; set; } = TenantPlan.STARTER;
    public int ServiceAlertDays { get; set; } = 15;
    public string? WarrantyPolicyText { get; set; }
    public int UsedWarrantyDays { get; set; } = 30;
    public bool CloserCommissionEnabled { get; set; }
    public CommissionType CloserCommissionType { get; set; }
    public decimal CloserCommissionValue { get; set; }
    public bool WholesalePriceEnabled { get; set; }
    public bool ArsTrCommissionEnabled { get; set; }
    public decimal ArsTrCommissionPercentage { get; set; }

    public ICollection<TenantUser> Users { get; set; } = [];
    public ICollection<Entity> Entities { get; set; } = [];
    public ICollection<StockItem> StockItems { get; set; } = [];
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<Purchase> Purchases { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}

// ─── TENANT USER ───────────────────────────────────────────
public class TenantUser : TenantEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InvitedEmail { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.EMPLOYEE;
    public List<string> AllowedModules { get; set; } = [];
    public bool IsCloser { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}

// ─── TENANT INVITATION ─────────────────────────────────────
public class TenantInvitation : TenantEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid Token { get; set; } = Guid.NewGuid();
    public Guid InvitedByUserId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.PENDING;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public Tenant Tenant { get; set; } = null!;
}

// ─── ENTITY (polymorphic: clients, providers, technicians, couriers) ──
public class Entity : TenantEntity
{
    public EntityType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Instagram { get; set; }
    public string? DocumentNumber { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressStreet { get; set; }
    public string? ShippingBranch { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingProvince { get; set; }
    public string? ShippingNotes { get; set; }
    public string? PreferredTransport { get; set; }
    public Guid? PriceListId { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public EntityBalance? Balance { get; set; }
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
    public ICollection<Purchase> Purchases { get; set; } = [];
}

// ─── CATALOG MODELS ────────────────────────────────────────
public class CatalogModel : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string IdType { get; set; } = "IMEI";
    public bool RequiresStorage { get; set; }
    public bool RequiresColor { get; set; }
    public bool RequiresSize { get; set; }
}

// ─── CATALOG ACCESSORIES ───────────────────────────────────
public class CatalogAccessory : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool RequiresModel { get; set; }
    public bool RequiresColor { get; set; }
}

// ─── CATALOG LOCATION ──────────────────────────────────────
public class CatalogLocation : TenantEntity
{
    public string Name { get; set; } = string.Empty;
}

// ─── STOCK ITEM ────────────────────────────────────────────
public class StockItem : TenantEntity
{
    public Guid ModelId { get; set; }
    public string? InternalCode { get; set; }
    public string? ImeiSerial { get; set; }
    public string? Color { get; set; }
    public int? StorageGb { get; set; }
    public decimal? SizeMm { get; set; }
    public StockCondition Condition { get; set; }
    public string? ConditionGrade { get; set; }
    public int? BatteryPct { get; set; }
    public decimal CostUsd { get; set; }
    public decimal SuggestedPriceUsd { get; set; }
    public decimal? WholesalePriceUsd { get; set; }
    public string? AccountEmail { get; set; }
    public StockStatus Status { get; set; } = StockStatus.AVAILABLE;
    public Guid? LocationId { get; set; }
    public Guid? PurchaseId { get; set; }
    public Guid? TradeInSaleId { get; set; }
    public string? Notes { get; set; }

    public CatalogModel Model { get; set; } = null!;
    public CatalogLocation? Location { get; set; }
    public Purchase? Purchase { get; set; }
}

// ─── STOCK BULK ────────────────────────────────────────────
public class StockBulk : TenantEntity
{
    public Guid AccessoryId { get; set; }
    public Guid? ModelId { get; set; }
    public Guid? LocationId { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public decimal CostUsd { get; set; }
    public decimal SuggestedPriceUsd { get; set; }

    public CatalogAccessory Accessory { get; set; } = null!;
    public CatalogLocation? Location { get; set; }
}

// ─── SALE ──────────────────────────────────────────────────
public class Sale : TenantEntity
{
    public Guid? EntityId { get; set; }
    public string? RetailClientName { get; set; }
    public string? RetailClientPhone { get; set; }
    public string? RetailClientInstagram { get; set; }
    public SaleCategory SaleCategory { get; set; }
    public SaleOrigin Origin { get; set; }
    public decimal TotalUsd { get; set; }
    public int WarrantyDays { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.COMPLETED;
    public string? Notes { get; set; }
    public List<Guid> CloserIds { get; set; } = [];
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public Entity? Entity { get; set; }
    public ICollection<SaleItem> Items { get; set; } = [];
    public ICollection<TransactionPayment> Payments { get; set; } = [];
    public ICollection<SaleCloserCommission> Commissions { get; set; } = [];
}

// ─── SALE ITEM ─────────────────────────────────────────────
public class SaleItem : TenantEntity
{
    public Guid SaleId { get; set; }
    public ItemKind Type { get; set; }
    public Guid? StockItemId { get; set; }
    public Guid? StockBulkId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal PriceUsd { get; set; }

    public Sale Sale { get; set; } = null!;
    public StockItem? StockItem { get; set; }
    public StockBulk? StockBulk { get; set; }
}

// ─── PURCHASE ──────────────────────────────────────────────
public class Purchase : TenantEntity
{
    public Guid? ProviderId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal TotalUsd { get; set; }
    public PurchaseType Type { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.ACTIVE;
    public string? Notes { get; set; }

    public Entity? Provider { get; set; }
    public ICollection<StockItem> StockItems { get; set; } = [];
    public ICollection<TransactionPayment> Payments { get; set; } = [];
}

// ─── RESERVATION ───────────────────────────────────────────
public class Reservation : TenantEntity
{
    public Guid? EntityId { get; set; }
    public string? RetailClientName { get; set; }
    public string? RetailClientPhone { get; set; }
    public string? RetailClientInstagram { get; set; }
    public Guid? StockItemId { get; set; }
    public Guid? StockBulkId { get; set; }
    public SaleCategory SaleCategory { get; set; }
    public DateTime PickupDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.ACTIVE;
    public decimal DepositAmountUsd { get; set; }
    public decimal AgreedPriceUsd { get; set; }
    public string? Notes { get; set; }

    public Entity? Entity { get; set; }
    public StockItem? StockItem { get; set; }
    public StockBulk? StockBulk { get; set; }
    public ICollection<TransactionPayment> Payments { get; set; } = [];
}

// ─── IMPORT SUPPLIER ORDER ─────────────────────────────────
public class ImportSupplierOrder : TenantEntity
{
    public Guid? ProviderId { get; set; }
    public Guid? CourierId { get; set; }
    public ImportOrderStatus Status { get; set; } = ImportOrderStatus.PENDING;
    public string? Notes { get; set; }
    public decimal TotalUsd { get; set; }

    public Entity? Provider { get; set; }
    public Entity? Courier { get; set; }
    public ICollection<ImportSupplierOrderItem> Items { get; set; } = [];
}

public class ImportSupplierOrderItem : TenantEntity
{
    public Guid OrderId { get; set; }
    public Guid? ModelId { get; set; }
    public Guid? AccessoryId { get; set; }
    public StockItemConditionDetail Condition { get; set; }
    public int Quantity { get; set; }
    public decimal CostProvUsd { get; set; }
    public int WeightGrams { get; set; }
    public decimal CourierUsdPerKg { get; set; }
    public decimal FleteUsd { get; set; }
    public decimal CostoFinalUsd { get; set; }
    public string? Notes { get; set; }
    public string? DetailObs { get; set; }

    public ImportSupplierOrder Order { get; set; } = null!;
}

// ─── CAJA ──────────────────────────────────────────────────
public class Caja : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmployeeBox { get; set; }
    public Guid? EmployeeUserId { get; set; }

    public ICollection<CashMovement> Movements { get; set; } = [];
}

// ─── CASH MOVEMENT ─────────────────────────────────────────
public class CashMovement : TenantEntity
{
    public Guid CajaId { get; set; }
    public CashMovementType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountUsd { get; set; }
    public decimal ExchangeRateUsd { get; set; } = 1;
    public Currency Currency { get; set; }
    public Guid? MovementCategoryId { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Detail { get; set; }

    public Caja Caja { get; set; } = null!;
    public MovementCategory? Category { get; set; }
}

// ─── CASH CLOSING ──────────────────────────────────────────
public class CashClosing : TenantEntity
{
    public DateOnly FechaCierre { get; set; }
    public decimal IngresosHoy { get; set; }
    public decimal EgresosHoy { get; set; }
    public decimal LiquidezFinalUsd { get; set; }
}

// ─── MOVEMENT CATEGORY ─────────────────────────────────────
public class MovementCategory : TenantEntity
{
    public string Name { get; set; } = string.Empty;
}

// ─── TRANSACTION PAYMENT ───────────────────────────────────
public class TransactionPayment : TenantEntity
{
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountUsd { get; set; }
    public decimal ExchangeRateUsd { get; set; } = 1;
}

// ─── SERVICE CLIENT JOB ────────────────────────────────────
public class ServiceClientJob : TenantEntity
{
    public string SvCode { get; set; } = string.Empty;
    public string RetailClientName { get; set; } = string.Empty;
    public string? RetailClientPhone { get; set; }
    public string? DeviceModel { get; set; }
    public string? ImeiSerial { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public Guid? TechnicianId { get; set; }
    public decimal PriceToClientUsd { get; set; }
    public decimal TechnicianCostUsd { get; set; }
    public PaymentMethod? DepositMethod { get; set; }
    public decimal DepositAmount { get; set; }
    public ServiceJobStatus Status { get; set; } = ServiceJobStatus.OPEN;
    public DateOnly? LimitDate { get; set; }

    public Entity? Technician { get; set; }
}

// ─── SERVICE STOCK JOB ─────────────────────────────────────
public class ServiceStockJob : TenantEntity
{
    public Guid StockItemId { get; set; }
    public Guid? TechnicianId { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public decimal AgreedCostUsd { get; set; }
    public ServiceJobStatus Status { get; set; } = ServiceJobStatus.OPEN;
    public DateOnly? LimitDate { get; set; }

    public StockItem StockItem { get; set; } = null!;
    public Entity? Technician { get; set; }
}

// ─── ENTITY BALANCE ────────────────────────────────────────
public class EntityBalance
{
    public Guid EntityId { get; set; }
    public decimal BalanceUsd { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Entity Entity { get; set; } = null!;
}

// ─── PRICE LIST ────────────────────────────────────────────
public class PriceList : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<PriceListItem> Items { get; set; } = [];
}

public class PriceListItem : TenantEntity
{
    public Guid PriceListId { get; set; }
    public ItemKind ItemKind { get; set; }
    public Guid ItemId { get; set; }
    public decimal PriceUsd { get; set; }

    public PriceList PriceList { get; set; } = null!;
}

// ─── RETENTION RULE ────────────────────────────────────────
public class RetentionRule : TenantEntity
{
    public string RuleType { get; set; } = string.Empty;
    public int DaysAfterSale { get; set; }
    public string MessageTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// ─── CALENDAR EVENT ────────────────────────────────────────
public class CalendarEvent : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Type { get; set; } = "MEETING";
}

// ─── SALE CLOSER COMMISSION ────────────────────────────────
public class SaleCloserCommission : TenantEntity
{
    public Guid CloserUserId { get; set; }
    public Guid SaleId { get; set; }
    public decimal AmountUsd { get; set; }
    public string PeriodMonth { get; set; } = string.Empty;
    public CommissionStatus Status { get; set; } = CommissionStatus.PENDING;

    public Sale Sale { get; set; } = null!;
}

// ─── OBJECTION RESPONSE ────────────────────────────────────
public class ObjectionResponse : TenantEntity
{
    public string RawText { get; set; } = string.Empty;
    public Guid? ClusterId { get; set; }
    public string Source { get; set; } = "MANUAL";
    public Guid? SaleId { get; set; }
    public string? SuggestedResponse { get; set; }
}

// ─── COMPETITOR ────────────────────────────────────────────
public class Competitor : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<CompetitorPrice> Prices { get; set; } = [];
}

public class CompetitorPrice : TenantEntity
{
    public Guid CompetitorId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int? StorageGb { get; set; }
    public decimal PriceUsd { get; set; }
}

// ─── TRADE-IN VALUATION ────────────────────────────────────
public class TradeInBaseValuation : TenantEntity
{
    public string ModelName { get; set; } = string.Empty;
    public int StorageGb { get; set; }
    public string BatteryRange { get; set; } = string.Empty;
    public decimal BaseValueUsd { get; set; }
}

// ─── BARCODE MAPPING ───────────────────────────────────────
public class BarcodeMapping : TenantEntity
{
    public string Barcode { get; set; } = string.Empty;
    public Guid ModelId { get; set; }
    public int? StorageGb { get; set; }
    public string? Color { get; set; }
    public StockCondition? Condition { get; set; }
}
