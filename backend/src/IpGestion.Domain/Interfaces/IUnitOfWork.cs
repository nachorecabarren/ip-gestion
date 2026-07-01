using IpGestion.Domain.Entities;

namespace IpGestion.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<Tenant> Tenants { get; }
    IRepository<Entity> Entities { get; }
    IRepository<StockItem> StockItems { get; }
    IRepository<StockBulk> StockBulks { get; }
    IRepository<Sale> Sales { get; }
    IRepository<SaleItem> SaleItems { get; }
    IRepository<Purchase> Purchases { get; }
    IRepository<Reservation> Reservations { get; }
    IRepository<ImportSupplierOrder> ImportOrders { get; }
    IRepository<ImportSupplierOrderItem> ImportOrderItems { get; }
    IRepository<Caja> Cajas { get; }
    IRepository<CashMovement> CashMovements { get; }
    IRepository<CashClosing> CashClosings { get; }
    IRepository<MovementCategory> MovementCategories { get; }
    IRepository<TransactionPayment> Payments { get; }
    IRepository<ServiceClientJob> ServiceClientJobs { get; }
    IRepository<ServiceStockJob> ServiceStockJobs { get; }
    IRepository<EntityBalance> EntityBalances { get; }
    IRepository<PriceList> PriceLists { get; }
    IRepository<RetentionRule> RetentionRules { get; }
    IRepository<CalendarEvent> CalendarEvents { get; }
    IRepository<SaleCloserCommission> Commissions { get; }
    IRepository<ObjectionResponse> Objections { get; }
    IRepository<Competitor> Competitors { get; }
    IRepository<TradeInBaseValuation> TradeInValuations { get; }
    IRepository<CatalogModel> CatalogModels { get; }
    IRepository<CatalogAccessory> CatalogAccessories { get; }
    IRepository<CatalogLocation> CatalogLocations { get; }
    IRepository<BarcodeMapping> BarcodeMappings { get; }
    IRepository<TenantUser> TenantUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
