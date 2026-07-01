using Microsoft.EntityFrameworkCore;
using IpGestion.Domain.Entities;
using IpGestion.Domain.Interfaces;

namespace IpGestion.Infrastructure.Persistence.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected readonly AppDbContext _db = db;
    protected readonly DbSet<T> _set = db.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private readonly AppDbContext _db = db;

    public IRepository<Tenant> Tenants => new Repository<Tenant>(_db);
    public IRepository<Domain.Entities.Entity> Entities => new Repository<Domain.Entities.Entity>(_db);
    public IRepository<StockItem> StockItems => new Repository<StockItem>(_db);
    public IRepository<StockBulk> StockBulks => new Repository<StockBulk>(_db);
    public IRepository<Sale> Sales => new Repository<Sale>(_db);
    public IRepository<SaleItem> SaleItems => new Repository<SaleItem>(_db);
    public IRepository<Purchase> Purchases => new Repository<Purchase>(_db);
    public IRepository<Reservation> Reservations => new Repository<Reservation>(_db);
    public IRepository<ImportSupplierOrder> ImportOrders => new Repository<ImportSupplierOrder>(_db);
    public IRepository<ImportSupplierOrderItem> ImportOrderItems => new Repository<ImportSupplierOrderItem>(_db);
    public IRepository<Caja> Cajas => new Repository<Caja>(_db);
    public IRepository<CashMovement> CashMovements => new Repository<CashMovement>(_db);
    public IRepository<CashClosing> CashClosings => new Repository<CashClosing>(_db);
    public IRepository<MovementCategory> MovementCategories => new Repository<MovementCategory>(_db);
    public IRepository<TransactionPayment> Payments => new Repository<TransactionPayment>(_db);
    public IRepository<ServiceClientJob> ServiceClientJobs => new Repository<ServiceClientJob>(_db);
    public IRepository<ServiceStockJob> ServiceStockJobs => new Repository<ServiceStockJob>(_db);
    public IRepository<EntityBalance> EntityBalances => new Repository<EntityBalance>(_db);
    public IRepository<PriceList> PriceLists => new Repository<PriceList>(_db);
    public IRepository<RetentionRule> RetentionRules => new Repository<RetentionRule>(_db);
    public IRepository<CalendarEvent> CalendarEvents => new Repository<CalendarEvent>(_db);
    public IRepository<SaleCloserCommission> Commissions => new Repository<SaleCloserCommission>(_db);
    public IRepository<ObjectionResponse> Objections => new Repository<ObjectionResponse>(_db);
    public IRepository<Competitor> Competitors => new Repository<Competitor>(_db);
    public IRepository<TradeInBaseValuation> TradeInValuations => new Repository<TradeInBaseValuation>(_db);
    public IRepository<CatalogModel> CatalogModels => new Repository<CatalogModel>(_db);
    public IRepository<CatalogAccessory> CatalogAccessories => new Repository<CatalogAccessory>(_db);
    public IRepository<CatalogLocation> CatalogLocations => new Repository<CatalogLocation>(_db);
    public IRepository<BarcodeMapping> BarcodeMappings => new Repository<BarcodeMapping>(_db);
    public IRepository<TenantUser> TenantUsers => new Repository<TenantUser>(_db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync() => await _db.DisposeAsync();
}
