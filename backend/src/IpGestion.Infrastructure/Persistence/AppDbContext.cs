using Microsoft.EntityFrameworkCore;
using IpGestion.Domain.Entities;

namespace IpGestion.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<TenantInvitation> TenantInvitations => Set<TenantInvitation>();
    public DbSet<Domain.Entities.Entity> Entities => Set<Domain.Entities.Entity>();
    public DbSet<CatalogModel> CatalogModels => Set<CatalogModel>();
    public DbSet<CatalogAccessory> CatalogAccessories => Set<CatalogAccessory>();
    public DbSet<CatalogLocation> CatalogLocations => Set<CatalogLocation>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockBulk> StockBulks => Set<StockBulk>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ImportSupplierOrder> ImportOrders => Set<ImportSupplierOrder>();
    public DbSet<ImportSupplierOrderItem> ImportOrderItems => Set<ImportSupplierOrderItem>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<CashClosing> CashClosings => Set<CashClosing>();
    public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();
    public DbSet<TransactionPayment> TransactionPayments => Set<TransactionPayment>();
    public DbSet<ServiceClientJob> ServiceClientJobs => Set<ServiceClientJob>();
    public DbSet<ServiceStockJob> ServiceStockJobs => Set<ServiceStockJob>();
    public DbSet<EntityBalance> EntityBalances => Set<EntityBalance>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<RetentionRule> RetentionRules => Set<RetentionRule>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<SaleCloserCommission> SaleCloserCommissions => Set<SaleCloserCommission>();
    public DbSet<ObjectionResponse> ObjectionResponses => Set<ObjectionResponse>();
    public DbSet<Competitor> Competitors => Set<Competitor>();
    public DbSet<CompetitorPrice> CompetitorPrices => Set<CompetitorPrice>();
    public DbSet<TradeInBaseValuation> TradeInValuations => Set<TradeInBaseValuation>();
    public DbSet<BarcodeMapping> BarcodeMappings => Set<BarcodeMapping>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // EntityBalance — keyless-ish (PK is EntityId)
        b.Entity<EntityBalance>().HasKey(e => e.EntityId);

        // JSON columns for list properties
        b.Entity<TenantUser>()
            .Property(e => e.AllowedModules)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
            )
            .HasColumnType("jsonb");

        b.Entity<Sale>()
            .Property(e => e.CloserIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
            )
            .HasColumnType("jsonb");
        b.Entity<Competitor>().Ignore(e => e.Prices); // loaded via CompetitorPrice

        // Soft precision for decimals
        foreach (var p in b.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            p.SetPrecision(18);
            p.SetScale(4);
        }

        // Auth: a user's email is unique globally (login uses email alone)
        b.Entity<TenantUser>().HasIndex(u => u.Email).IsUnique();
        // Invitation tokens are unique; index by tenant for listing pending ones
        b.Entity<TenantInvitation>().HasIndex(i => i.Token).IsUnique();
        b.Entity<TenantInvitation>().HasIndex(i => new { i.TenantId, i.Status });

        // Global query filter placeholder — extend per tenant in repos
        b.Entity<Tenant>().HasIndex(t => t.Name);
        b.Entity<Domain.Entities.Entity>().HasIndex(e => new { e.TenantId, e.Type });
        b.Entity<StockItem>().HasIndex(s => new { s.TenantId, s.Status });
        b.Entity<Sale>().HasIndex(s => new { s.TenantId, s.SaleDate });
        b.Entity<ServiceClientJob>().HasIndex(s => new { s.TenantId, s.Status });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        return base.SaveChangesAsync(ct);
    }
}
