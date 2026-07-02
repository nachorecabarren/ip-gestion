using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using IpGestion.Application.Common.DTOs;
using IpGestion.Application.Common.Exceptions;
using IpGestion.Application.Interfaces;
using IpGestion.Domain.Enums;
using IpGestion.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace IpGestion.Infrastructure.Services;

// ─── DASHBOARD ─────────────────────────────────────────────
public class DashboardService(AppDbContext db) : IDashboardService
{
    public async Task<DashboardKpisDto> GetKpisAsync(Guid tenantId, string periodo, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var from = periodo switch
        {
            "week" => now.AddDays(-7),
            "month" => new DateTime(now.Year, now.Month, 1),
            "year" => new DateTime(now.Year, 1, 1),
            _ => new DateTime(now.Year, now.Month, 1)
        };

        var sales = await db.Sales
            .Where(s => s.TenantId == tenantId && s.SaleDate >= from && s.Status == SaleStatus.COMPLETED)
            .Include(s => s.Items).ThenInclude(i => i.StockItem)
            .ToListAsync(ct);

        var facturacion = sales.Sum(s => s.TotalUsd);
        var costo = sales.SelectMany(s => s.Items)
            .Where(i => i.StockItem != null)
            .Sum(i => i.StockItem!.CostUsd * i.Quantity);
        var margen = facturacion - costo;

        var gastoClosers = await db.SaleCloserCommissions
            .Where(c => c.TenantId == tenantId && c.Sale.SaleDate >= from)
            .SumAsync(c => c.AmountUsd, ct);

        var gastoOp = await db.CashMovements
            .Where(m => m.TenantId == tenantId && m.Type == CashMovementType.EXPENSE && m.CreatedAt >= from)
            .SumAsync(m => m.AmountUsd, ct);

        var stock = await db.StockItems
            .CountAsync(s => s.TenantId == tenantId && s.Status == StockStatus.AVAILABLE, ct);

        var reservas = await db.Reservations
            .CountAsync(r => r.TenantId == tenantId && r.Status == ReservationStatus.ACTIVE, ct);

        return new DashboardKpisDto(
            facturacion, margen, margen - gastoClosers - gastoOp,
            sales.Count, stock, reservas, gastoClosers, gastoOp, 0, periodo
        );
    }

    public async Task<IEnumerable<QuickSaleDto>> GetRecentSalesAsync(Guid tenantId, int count = 10, CancellationToken ct = default)
    {
        var sales = await db.Sales
            .Where(s => s.TenantId == tenantId && s.Status == SaleStatus.COMPLETED)
            .Include(s => s.Items).ThenInclude(i => i.StockItem).ThenInclude(si => si!.Model)
            .Include(s => s.Entity)
            .OrderByDescending(s => s.SaleDate)
            .Take(count)
            .ToListAsync(ct);

        return sales.Select(s => new QuickSaleDto(
            s.SaleDate,
            s.Items.FirstOrDefault()?.StockItem?.Model?.Name ?? "Accesorio",
            s.Entity?.Name ?? s.RetailClientName ?? "Consumidor Final",
            s.TotalUsd,
            s.TotalUsd - s.Items.Where(i => i.StockItem != null).Sum(i => i.StockItem!.CostUsd)
        ));
    }
}

// ─── ENTITY SERVICE ────────────────────────────────────────
public class EntityService(AppDbContext db) : IEntityService
{
    public async Task<PagedResult<EntityDto>> GetPagedAsync(Guid tenantId, EntityType? type, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Entities.Where(e => e.TenantId == tenantId);
        if (type.HasValue) q = q.Where(e => e.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.Name.Contains(search) || (e.Phone != null && e.Phone.Contains(search)));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .GroupJoin(db.EntityBalances, e => e.Id, b => b.EntityId, (e, balances) => new { e, balances })
            .SelectMany(x => x.balances.DefaultIfEmpty(), (x, b) => new EntityDto(
                x.e.Id, x.e.Type, x.e.Name, x.e.Phone, x.e.Email,
                x.e.Instagram, x.e.DocumentNumber, x.e.AddressCity, x.e.IsActive,
                b == null ? 0 : b.BalanceUsd))
            .ToListAsync(ct);

        return new PagedResult<EntityDto>(items, total, page, pageSize);
    }

    public async Task<EntityDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await db.Entities.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (e == null) return null;
        var bal = await db.EntityBalances.FindAsync([id], ct);
        return new EntityDto(e.Id, e.Type, e.Name, e.Phone, e.Email, e.Instagram, e.DocumentNumber, e.AddressCity, e.IsActive, bal?.BalanceUsd ?? 0);
    }

    public async Task<EntityDto> CreateAsync(Guid tenantId, CreateEntityDto dto, CancellationToken ct = default)
    {
        var entity = new Domain.Entities.Entity
        {
            TenantId = tenantId,
            Type = dto.Type,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            Instagram = dto.Instagram,
            DocumentNumber = dto.DocumentNumber,
            AddressCity = dto.AddressCity,
            AddressStreet = dto.AddressStreet,
            ShippingCity = dto.ShippingCity,
            ShippingProvince = dto.ShippingProvince,
            ShippingBranch = dto.ShippingBranch,
            ShippingPostalCode = dto.ShippingPostalCode,
            ShippingNotes = dto.ShippingNotes,
            PreferredTransport = dto.PreferredTransport
        };
        db.Entities.Add(entity);
        db.EntityBalances.Add(new Domain.Entities.EntityBalance { EntityId = entity.Id, BalanceUsd = 0 });
        await db.SaveChangesAsync(ct);
        return new EntityDto(entity.Id, entity.Type, entity.Name, entity.Phone, entity.Email,
            entity.Instagram, entity.DocumentNumber, entity.AddressCity, entity.IsActive, 0);
    }

    public async Task<EntityDto> UpdateAsync(Guid tenantId, Guid id, UpdateEntityDto dto, CancellationToken ct = default)
    {
        var entity = await db.Entities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Entity), id);
        entity.Name = dto.Name; entity.Phone = dto.Phone; entity.Email = dto.Email;
        entity.Instagram = dto.Instagram; entity.DocumentNumber = dto.DocumentNumber;
        entity.AddressCity = dto.AddressCity; entity.AddressStreet = dto.AddressStreet;
        entity.IsActive = dto.IsActive;
        await db.SaveChangesAsync(ct);
        var bal = await db.EntityBalances.FindAsync([id], ct);
        return new EntityDto(entity.Id, entity.Type, entity.Name, entity.Phone, entity.Email,
            entity.Instagram, entity.DocumentNumber, entity.AddressCity, entity.IsActive, bal?.BalanceUsd ?? 0);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await db.Entities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Entity), id);
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}

// ─── STOCK SERVICE ─────────────────────────────────────────
public class StockService(AppDbContext db) : IStockService
{
    private static StockItemDto Map(Domain.Entities.StockItem s) => new(
        s.Id, s.Model?.Name ?? "", s.InternalCode, s.ImeiSerial, s.Color, s.StorageGb,
        s.Condition, s.ConditionGrade, s.BatteryPct, s.CostUsd, s.SuggestedPriceUsd,
        s.WholesalePriceUsd, s.Status, s.Location?.Name, s.Notes, s.CreatedAt);

    public async Task<PagedResult<StockItemDto>> GetItemsPagedAsync(Guid tenantId, StockStatus? status, StockCondition? condition, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.StockItems.Include(s => s.Model).Include(s => s.Location)
            .Where(s => s.TenantId == tenantId);
        if (status.HasValue) q = q.Where(s => s.Status == status.Value);
        if (condition.HasValue) q = q.Where(s => s.Condition == condition.Value);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => (s.ImeiSerial != null && s.ImeiSerial.Contains(search)) ||
                             (s.InternalCode != null && s.InternalCode.Contains(search)) ||
                             s.Model.Name.Contains(search));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<StockItemDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<StockItemDto?> GetItemByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var s = await db.StockItems.Include(x => x.Model).Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return s == null ? null : Map(s);
    }

    public async Task<StockItemDto?> GetByBarcodeAsync(Guid tenantId, string barcode, CancellationToken ct = default)
    {
        var mapping = await db.BarcodeMappings
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Barcode == barcode, ct);
        if (mapping == null) return null;
        var item = await db.StockItems.Include(s => s.Model).Include(s => s.Location)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ModelId == mapping.ModelId && s.Status == StockStatus.AVAILABLE, ct);
        return item == null ? null : Map(item);
    }

    public async Task<StockItemDto> CreateItemAsync(Guid tenantId, CreateStockItemDto dto, CancellationToken ct = default)
    {
        var model = await db.CatalogModels.FirstOrDefaultAsync(m => m.TenantId == tenantId && m.Id == dto.ModelId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.CatalogModel), dto.ModelId);
        var item = new Domain.Entities.StockItem
        {
            TenantId = tenantId,
            ModelId = dto.ModelId,
            ImeiSerial = dto.ImeiSerial,
            Color = dto.Color,
            StorageGb = dto.StorageGb,
            Condition = dto.Condition,
            BatteryPct = dto.BatteryPct,
            CostUsd = dto.CostUsd,
            SuggestedPriceUsd = dto.SuggestedPriceUsd,
            WholesalePriceUsd = dto.WholesalePriceUsd,
            LocationId = dto.LocationId,
            Notes = dto.Notes,
            Status = Domain.Enums.StockStatus.AVAILABLE,
            InternalCode = $"IP-{DateTime.UtcNow.Ticks % 100000}",
        };
        db.StockItems.Add(item);
        await db.SaveChangesAsync(ct);
        item.Model = model;
        item.Location = dto.LocationId.HasValue
            ? await db.CatalogLocations.FindAsync([dto.LocationId.Value], ct)
            : null;
        return Map(item);
    }

    public async Task<StockItemDto> UpdateItemAsync(Guid tenantId, Guid id, UpdateStockItemDto dto, CancellationToken ct = default)
    {
        var item = await db.StockItems.Include(s => s.Model).Include(s => s.Location)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.StockItem), id);
        item.Color = dto.Color; item.StorageGb = dto.StorageGb; item.Condition = dto.Condition;
        item.ConditionGrade = dto.ConditionGrade; item.BatteryPct = dto.BatteryPct;
        item.SuggestedPriceUsd = dto.SuggestedPriceUsd; item.WholesalePriceUsd = dto.WholesalePriceUsd;
        item.LocationId = dto.LocationId; item.Notes = dto.Notes;
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task VoidItemAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var item = await db.StockItems.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.StockItem), id);
        item.Status = StockStatus.VOIDED;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<StockBulkDto>> GetBulkItemsAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await db.StockBulks.Include(b => b.Accessory).Include(b => b.Location)
            .Where(b => b.TenantId == tenantId)
            .Select(b => new StockBulkDto(b.Id, b.Accessory.Name, b.Color, b.Quantity, b.LowStockThreshold,
                b.CostUsd, b.SuggestedPriceUsd, b.Location != null ? b.Location.Name : null))
            .ToListAsync(ct);
    }

    public async Task<TradeInQuoteDto> GetTradeInQuoteAsync(Guid tenantId, TradeInQuoteRequestDto req, CancellationToken ct = default)
    {
        var val = await db.TradeInValuations
            .Where(v => v.TenantId == tenantId && v.ModelName == req.ModelName && v.StorageGb == req.StorageGb)
            .FirstOrDefaultAsync(ct);
        var baseVal = val?.BaseValueUsd ?? 0;
        var adj = req.BatteryPct < 80 ? baseVal * 0.85m : baseVal;
        return new TradeInQuoteDto(baseVal, adj, req.BatteryPct < 80 ? "Descuento por batería < 80%" : "");
    }

    public async Task BulkUpdatePricesAsync(Guid tenantId, List<Guid> itemIds, decimal newPrice, CancellationToken ct = default)
    {
        var items = await db.StockItems.Where(s => s.TenantId == tenantId && itemIds.Contains(s.Id)).ToListAsync(ct);
        items.ForEach(i => i.SuggestedPriceUsd = newPrice);
        await db.SaveChangesAsync(ct);
    }

    public async Task TransferStockAsync(Guid tenantId, List<Guid> itemIds, Guid targetLocationId, CancellationToken ct = default)
    {
        var items = await db.StockItems.Where(s => s.TenantId == tenantId && itemIds.Contains(s.Id)).ToListAsync(ct);
        items.ForEach(i => i.LocationId = targetLocationId);
        await db.SaveChangesAsync(ct);
    }
}

// ─── SALE SERVICE ──────────────────────────────────────────
public class SaleService(AppDbContext db) : ISaleService
{
    public async Task<PagedResult<SaleDto>> GetPagedAsync(Guid tenantId, SaleCategory? category, SaleOrigin? origin, string? search, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Sales.Include(s => s.Items).ThenInclude(i => i.StockItem).ThenInclude(si => si!.Model)
            .Include(s => s.Payments).Include(s => s.Entity)
            .Where(s => s.TenantId == tenantId);
        if (category.HasValue) q = q.Where(s => s.SaleCategory == category.Value);
        if (origin.HasValue) q = q.Where(s => s.Origin == origin.Value);
        if (from.HasValue) q = q.Where(s => s.SaleDate >= from.Value);
        if (to.HasValue) q = q.Where(s => s.SaleDate <= to.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(s => (s.RetailClientName != null && s.RetailClientName.Contains(search)) ||
                             (s.Entity != null && s.Entity.Name.Contains(search)));
        var total = await q.CountAsync(ct);
        var sales = await q.OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var userNames = await ResolveCloserNames(tenantId, sales, ct);
        return new PagedResult<SaleDto>(sales.Select(s => MapSale(s, userNames)), total, page, pageSize);
    }

    public async Task<SaleDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var s = await db.Sales.Include(x => x.Items).ThenInclude(i => i.StockItem).ThenInclude(si => si!.Model)
            .Include(x => x.Payments).Include(x => x.Entity)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        if (s == null) return null;
        var userNames = await ResolveCloserNames(tenantId, [s], ct);
        return MapSale(s, userNames);
    }

    private async Task<Dictionary<Guid, string>> ResolveCloserNames(Guid tenantId, IEnumerable<Domain.Entities.Sale> sales, CancellationToken ct)
    {
        var allIds = sales.SelectMany(s => s.CloserIds).Distinct().ToList();
        if (allIds.Count == 0) return [];
        return await db.TenantUsers
            .Where(u => u.TenantId == tenantId && allIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
    }

    private static DateTime NormalizeToUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    public async Task<SaleDto> CreateAsync(Guid tenantId, CreateSaleDto dto, CancellationToken ct = default)
    {
        var sale = new Domain.Entities.Sale
        {
            TenantId = tenantId,
            EntityId = dto.EntityId,
            RetailClientName = dto.RetailClientName,
            RetailClientPhone = dto.RetailClientPhone,
            RetailClientInstagram = dto.RetailClientInstagram,
            SaleCategory = dto.SaleCategory,
            Origin = dto.Origin,
            TotalUsd = dto.TotalUsd,
            WarrantyDays = dto.WarrantyDays,
            Notes = dto.Notes,
            CloserIds = dto.CloserIds,
            SaleDate = NormalizeToUtc(dto.SaleDate),
            Status = SaleStatus.COMPLETED
        };

        // Add items + update stock
        foreach (var item in dto.Items)
        {
            sale.Items.Add(new Domain.Entities.SaleItem
            {
                TenantId = tenantId,
                Type = item.Type,
                StockItemId = item.StockItemId,
                StockBulkId = item.StockBulkId,
                Quantity = item.Quantity,
                PriceUsd = item.PriceUsd
            });
            if (item.StockItemId.HasValue)
            {
                var si = await db.StockItems.FindAsync([item.StockItemId.Value], ct);
                if (si != null) si.Status = StockStatus.SOLD;
            }
            if (item.StockBulkId.HasValue)
            {
                var sb = await db.StockBulks.FindAsync([item.StockBulkId.Value], ct);
                if (sb != null) sb.Quantity = Math.Max(0, sb.Quantity - item.Quantity);
            }
        }

        // Payments
        foreach (var p in dto.Payments)
            sale.Payments.Add(new Domain.Entities.TransactionPayment
            {
                TenantId = tenantId,
                ReferenceId = sale.Id,
                ReferenceType = "SALE",
                Method = p.Method,
                Currency = p.Currency,
                Amount = p.Amount,
                ExchangeRateUsd = p.ExchangeRateUsd,
                AmountUsd = p.Currency == Currency.USD ? p.Amount : p.Amount / p.ExchangeRateUsd
            });

        db.Sales.Add(sale);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, sale.Id, ct))!;
    }

    public async Task VoidSaleAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var sale = await db.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Sale), id);

        if (sale.Status == SaleStatus.VOIDED)
            throw new BusinessException("La venta ya está anulada.");

        sale.Status = SaleStatus.VOIDED;

        foreach (var item in sale.Items)
        {
            // ─── Restaurar equipo unitario ─────────────────────────
            if (item.StockItemId.HasValue)
            {
                var si = await db.StockItems.FindAsync([item.StockItemId.Value], ct);
                if (si != null)
                {
                    // Si vino de reserva → volver a RESERVED, sino AVAILABLE
                    var reservation = await db.Reservations.FirstOrDefaultAsync(r =>
                        r.TenantId == tenantId &&
                        r.StockItemId == si.Id &&
                        r.Status == ReservationStatus.SOLD, ct);

                    if (reservation != null)
                    {
                        reservation.Status = ReservationStatus.ACTIVE;
                        si.Status = StockStatus.RESERVED;
                    }
                    else
                    {
                        si.Status = StockStatus.AVAILABLE;
                    }
                }
            }

            // ─── Restaurar accesorio bulk ──────────────────────────
            if (item.StockBulkId.HasValue)
            {
                var sb = await db.StockBulks.FindAsync([item.StockBulkId.Value], ct);
                if (sb != null) sb.Quantity += item.Quantity;
            }
        }

        // ─── Revertir movimientos de caja ─────────────────────────
        var cajaMovements = await db.CashMovements
            .Where(m => m.TenantId == tenantId &&
                        m.ReferenceId == sale.Id &&
                        m.ReferenceType == "SALE")
            .ToListAsync(ct);

        foreach (var mov in cajaMovements)
        {
            // Crear movimiento inverso en lugar de borrar (para mantener historial)
            db.CashMovements.Add(new Domain.Entities.CashMovement
            {
                TenantId = tenantId,
                CajaId = mov.CajaId,
                Type = CashMovementType.EXPENSE,
                Method = mov.Method,
                Currency = mov.Currency,
                Amount = mov.Amount,
                AmountUsd = mov.AmountUsd,
                ExchangeRateUsd = mov.ExchangeRateUsd,
                ReferenceId = sale.Id,
                ReferenceType = "SALE_VOID",
                Detail = $"Reversión venta anulada #{sale.Id.ToString()[..8]}"
            });
        }

        // ─── Revertir saldo de cuenta corriente ───────────────────
        if (sale.EntityId.HasValue)
        {
            var balance = await db.EntityBalances.FindAsync([sale.EntityId.Value], ct);
            if (balance != null)
            {
                // Si la venta generó deuda (pagos parciales), revertirla
                var totalPagado = sale.Payments.Sum(p => p.AmountUsd);
                var diferencia = sale.TotalUsd - totalPagado;
                if (diferencia > 0)
                    balance.BalanceUsd -= diferencia; // quitar la deuda que quedaba
            }
        }

        // ─── Anular comisiones de closers ─────────────────────────
        var comisiones = await db.SaleCloserCommissions
            .Where(c => c.TenantId == tenantId && c.SaleId == sale.Id && c.Status == CommissionStatus.PENDING)
            .ToListAsync(ct);

        foreach (var com in comisiones)
            com.Status = CommissionStatus.PAID; // marcar como "anulada" reutilizando PAID
                                                // en producción agregar CommissionStatus.VOIDED

        await db.SaveChangesAsync(ct);
    }


    private static SaleDto MapSale(Domain.Entities.Sale s, Dictionary<Guid, string> userNames)
    {
        var costo = s.Items.Where(i => i.StockItem != null).Sum(i => i.StockItem!.CostUsd * i.Quantity);
        var soldBy = s.CloserIds.Count > 0 && userNames.TryGetValue(s.CloserIds[0], out var name) ? name : null;
        return new SaleDto(s.Id,
            s.Entity?.Name ?? s.RetailClientName, s.Entity?.Phone ?? s.RetailClientPhone,
            s.SaleCategory, s.Origin, s.TotalUsd, s.TotalUsd - costo, s.WarrantyDays, s.Status, s.Notes, s.SaleDate,
            s.Items.Select(i => new SaleItemDto(i.Id, i.Type,
                i.StockItem?.Model?.Name ?? "Accesorio", i.Quantity, i.PriceUsd, i.StockItem?.ImeiSerial)).ToList(),
            s.Payments.Select(p => new PaymentDto(p.Method, p.Currency, p.Amount, p.ExchangeRateUsd)).ToList(),
            soldBy
        );
    }
}

// ─── PURCHASE SERVICE ──────────────────────────────────────
public class PurchaseService(AppDbContext db) : IPurchaseService
{
    public async Task<PagedResult<PurchaseDto>> GetPagedAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Purchases.Include(p => p.Provider).Include(p => p.StockItems).ThenInclude(s => s.Model)
            .Where(p => p.TenantId == tenantId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(p => p.PurchaseDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<PurchaseDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<PurchaseDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var p = await db.Purchases.Include(x => x.Provider).Include(x => x.StockItems).ThenInclude(s => s.Model)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return p == null ? null : Map(p);
    }

    public async Task<PurchaseDto> CreateAsync(Guid tenantId, CreatePurchaseDto dto, CancellationToken ct = default)
    {
        var purchase = new Domain.Entities.Purchase
        {
            TenantId = tenantId,
            ProviderId = dto.ProviderId,
            PurchaseDate = dto.PurchaseDate,
            Type = dto.Type,
            Notes = dto.Notes,
            Status = PurchaseStatus.ACTIVE
        };

        foreach (var item in dto.DeviceItems)
        {
            var si = new Domain.Entities.StockItem
            {
                TenantId = tenantId,
                ModelId = item.ModelId,
                ImeiSerial = item.ImeiSerial,
                Color = item.Color,
                StorageGb = item.StorageGb,
                Condition = item.Condition,
                BatteryPct = item.BatteryPct,
                CostUsd = item.CostUsd,
                SuggestedPriceUsd = item.SuggestedPriceUsd,
                WholesalePriceUsd = item.WholesalePriceUsd,
                LocationId = item.LocationId,
                Notes = item.Notes,
                Status = StockStatus.AVAILABLE,
                InternalCode = $"IP-{DateTime.UtcNow.Ticks % 100000}"
            };
            purchase.StockItems.Add(si);
        }

        foreach (var bulk in dto.BulkItems)
        {
            var existing = await db.StockBulks.FirstOrDefaultAsync(b =>
                b.TenantId == tenantId && b.AccessoryId == bulk.AccessoryId && b.Color == bulk.Color, ct);
            if (existing != null)
                existing.Quantity += bulk.Quantity;
            else
                db.StockBulks.Add(new Domain.Entities.StockBulk
                {
                    TenantId = tenantId,
                    AccessoryId = bulk.AccessoryId,
                    ModelId = bulk.ModelId,
                    Color = bulk.Color,
                    Quantity = bulk.Quantity,
                    CostUsd = bulk.CostUsd,
                    SuggestedPriceUsd = bulk.SuggestedPriceUsd
                });
        }

        purchase.TotalUsd = dto.DeviceItems.Sum(i => i.CostUsd) + dto.BulkItems.Sum(i => i.CostUsd * i.Quantity);
        db.Purchases.Add(purchase);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, purchase.Id, ct))!;
    }

    public async Task VoidAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var p = await db.Purchases.Include(x => x.StockItems)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Purchase), id);
        p.Status = PurchaseStatus.CANCELLED;
        p.StockItems.ToList().ForEach(i => i.Status = StockStatus.VOIDED);
        await db.SaveChangesAsync(ct);
    }

    private static PurchaseDto Map(Domain.Entities.Purchase p) => new(
        p.Id, p.Provider?.Name, p.PurchaseDate, p.TotalUsd, p.Type, p.Status, p.Notes,
        p.StockItems.Select(s => new StockItemDto(s.Id, s.Model?.Name ?? "", s.InternalCode, s.ImeiSerial,
            s.Color, s.StorageGb, s.Condition, s.ConditionGrade, s.BatteryPct, s.CostUsd, s.SuggestedPriceUsd,
            s.WholesalePriceUsd, s.Status, null, s.Notes, s.CreatedAt)).ToList()
    );
}

// ─── RESERVATION SERVICE ───────────────────────────────────
public class ReservationService(AppDbContext db) : IReservationService
{
    public async Task<PagedResult<ReservationDto>> GetPagedAsync(Guid tenantId, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Reservations.Include(r => r.Entity).Include(r => r.StockItem).ThenInclude(s => s!.Model)
            .Where(r => r.TenantId == tenantId);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(r => r.PickupDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ReservationDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<ReservationDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var r = await db.Reservations.Include(x => x.Entity).Include(x => x.StockItem).ThenInclude(s => s!.Model)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return r == null ? null : Map(r);
    }

    public async Task<ReservationDto> CreateAsync(Guid tenantId, CreateReservationDto dto, CancellationToken ct = default)
    {
        var res = new Domain.Entities.Reservation
        {
            TenantId = tenantId,
            EntityId = dto.EntityId,
            RetailClientName = dto.RetailClientName,
            RetailClientPhone = dto.RetailClientPhone,
            StockItemId = dto.StockItemId,
            StockBulkId = dto.StockBulkId,
            SaleCategory = dto.SaleCategory,
            PickupDate = dto.PickupDate,
            AgreedPriceUsd = dto.AgreedPriceUsd,
            DepositAmountUsd = dto.DepositAmountUsd,
            Notes = dto.Notes,
            Status = ReservationStatus.ACTIVE
        };
        if (dto.StockItemId.HasValue)
        {
            var si = await db.StockItems.FindAsync([dto.StockItemId.Value], ct);
            if (si != null) si.Status = StockStatus.RESERVED;
        }
        db.Reservations.Add(res);
        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, res.Id, ct))!;
    }

    public async Task CancelAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var r = await db.Reservations.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Reservation), id);
        r.Status = ReservationStatus.CANCELLED;
        if (r.StockItemId.HasValue)
        {
            var si = await db.StockItems.FindAsync([r.StockItemId.Value], ct);
            if (si != null) si.Status = StockStatus.AVAILABLE;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<SaleDto> ConvertToSaleAsync(Guid tenantId, Guid reservationId, CreateSaleDto dto, CancellationToken ct = default)
    {
        var r = await db.Reservations.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == reservationId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Reservation), reservationId);
        var saleService = new SaleService(db);
        var sale = await saleService.CreateAsync(tenantId, dto with { Origin = SaleOrigin.RESERVATION }, ct);
        r.Status = ReservationStatus.SOLD;
        await db.SaveChangesAsync(ct);
        return sale;
    }

    private static ReservationDto Map(Domain.Entities.Reservation r) => new(
        r.Id, r.Entity?.Name ?? r.RetailClientName, r.Entity?.Phone ?? r.RetailClientPhone,
        r.StockItem?.Model?.Name ?? "Accesorio", r.SaleCategory, r.PickupDate,
        r.Status, r.DepositAmountUsd, r.AgreedPriceUsd, r.Notes, r.CreatedAt
    );
}

// ─── CAJA SERVICE ──────────────────────────────────────────
public class CajaService(AppDbContext db) : ICajaService
{
    public async Task<IEnumerable<CajaDto>> GetCajasAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cajas = await db.Cajas.Where(c => c.TenantId == tenantId && c.IsActive).ToListAsync(ct);
        var result = new List<CajaDto>();
        foreach (var caja in cajas)
        {
            var movs = await db.CashMovements.Where(m => m.CajaId == caja.Id).ToListAsync(ct);
            decimal Bal(PaymentMethod m) => movs.Where(x => x.Method == m)
                .Sum(x => x.Type == CashMovementType.INCOME || x.Type == CashMovementType.SALE ? x.Amount : -x.Amount);
            result.Add(new CajaDto(caja.Id, caja.Name, caja.IsDefault, caja.IsActive,
                Bal(PaymentMethod.USD_CASH), Bal(PaymentMethod.USDT), Bal(PaymentMethod.ARS_CASH), Bal(PaymentMethod.ARS_TR)));
        }
        return result;
    }

    public async Task<IEnumerable<CashMovementDto>> GetMovementsAsync(Guid tenantId, Guid? cajaId, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.CashMovements.Include(m => m.Caja).Include(m => m.Category)
            .Where(m => m.TenantId == tenantId);
        if (cajaId.HasValue) q = q.Where(m => m.CajaId == cajaId.Value);
        if (from.HasValue) q = q.Where(m => m.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(m => m.CreatedAt <= to.Value);
        return await q.OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new CashMovementDto(m.Id, m.Caja.Name, m.Type, m.Method, m.Amount,
                m.AmountUsd, m.Currency, m.Detail, m.Category != null ? m.Category.Name : null, m.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<CashMovementDto> RegisterMovementAsync(Guid tenantId, CreateCashMovementDto dto, CancellationToken ct = default)
    {
        var mov = new Domain.Entities.CashMovement
        {
            TenantId = tenantId,
            CajaId = dto.CajaId,
            Type = dto.Type,
            Method = dto.Method,
            Currency = dto.Currency,
            Amount = dto.Amount,
            ExchangeRateUsd = dto.ExchangeRateUsd,
            AmountUsd = dto.Currency == Currency.USD ? dto.Amount : dto.Amount / dto.ExchangeRateUsd,
            MovementCategoryId = dto.CategoryId,
            Detail = dto.Detail
        };
        db.CashMovements.Add(mov);
        await db.SaveChangesAsync(ct);
        var caja = await db.Cajas.FindAsync([dto.CajaId], ct);
        return new CashMovementDto(mov.Id, caja?.Name ?? "", mov.Type, mov.Method, mov.Amount,
            mov.AmountUsd, mov.Currency, mov.Detail, null, mov.CreatedAt);
    }

    public async Task CloseDayAsync(Guid tenantId, DateOnly date, CancellationToken ct = default)
    {
        var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var movs = await db.CashMovements.Where(m => m.TenantId == tenantId && m.CreatedAt >= from && m.CreatedAt <= to).ToListAsync(ct);
        var ing = movs.Where(m => m.Type is CashMovementType.INCOME or CashMovementType.SALE).Sum(m => m.AmountUsd);
        var eg = movs.Where(m => m.Type is CashMovementType.EXPENSE or CashMovementType.PURCHASE).Sum(m => m.AmountUsd);
        db.CashClosings.Add(new Domain.Entities.CashClosing
        {
            TenantId = tenantId,
            FechaCierre = date,
            IngresosHoy = ing,
            EgresosHoy = eg,
            LiquidezFinalUsd = ing - eg
        });
        await db.SaveChangesAsync(ct);
    }
}

// ─── SERVICE TECH ──────────────────────────────────────────
public class ServiceTechService(AppDbContext db) : IServiceTechService
{
    private async Task<string> NextSvCodeAsync(Guid tenantId, CancellationToken ct)
    {
        var count = await db.ServiceClientJobs.CountAsync(j => j.TenantId == tenantId, ct);
        return $"SV-{(count + 1):D3}";
    }

    public async Task<PagedResult<ServiceClientJobDto>> GetClientJobsPagedAsync(Guid tenantId, ServiceJobStatus? status, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.ServiceClientJobs.Include(j => j.Technician).Where(j => j.TenantId == tenantId);
        if (status.HasValue) q = q.Where(j => j.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(j => j.RetailClientName.Contains(search) || j.SvCode.Contains(search) ||
                             (j.ImeiSerial != null && j.ImeiSerial.Contains(search)));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(j => j.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ServiceClientJobDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<ServiceClientJobDto?> GetClientJobByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var j = await db.ServiceClientJobs.Include(x => x.Technician)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return j == null ? null : Map(j);
    }

    public async Task<ServiceClientJobDto> CreateClientJobAsync(Guid tenantId, CreateServiceClientJobDto dto, CancellationToken ct = default)
    {
        Guid? technicianId = dto.TechnicianId;
        if (dto.TechnicianId.HasValue)
        {
            var existingEntity = await db.Entities.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == dto.TechnicianId.Value, ct);
            if (existingEntity is not null)
            {
                technicianId = existingEntity.Id;
            }
            else
            {
                var teamUser = await db.TenantUsers.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == dto.TechnicianId.Value && u.IsActive, ct);
                if (teamUser is not null)
                {
                    var technicianEntity = new Domain.Entities.Entity
                    {
                        TenantId = tenantId,
                        Type = EntityType.TECHNICIAN,
                        Name = teamUser.DisplayName,
                        Email = teamUser.Email,
                        IsActive = true,
                    };
                    db.Entities.Add(technicianEntity);
                    await db.SaveChangesAsync(ct);
                    technicianId = technicianEntity.Id;
                }
            }
        }

        var job = new Domain.Entities.ServiceClientJob
        {
            TenantId = tenantId,
            SvCode = await NextSvCodeAsync(tenantId, ct),
            RetailClientName = dto.RetailClientName,
            RetailClientPhone = dto.RetailClientPhone,
            DeviceModel = dto.DeviceModel,
            ImeiSerial = dto.ImeiSerial,
            IssueDescription = dto.IssueDescription,
            TechnicianId = technicianId,
            PriceToClientUsd = dto.PriceToClientUsd,
            TechnicianCostUsd = dto.TechnicianCostUsd,
            DepositMethod = dto.DepositMethod,
            DepositAmount = dto.DepositAmount,
            LimitDate = dto.LimitDate,
            Status = ServiceJobStatus.OPEN
        };
        db.ServiceClientJobs.Add(job);
        await db.SaveChangesAsync(ct);
        return (await GetClientJobByIdAsync(tenantId, job.Id, ct))!;
    }

    public async Task<ServiceClientJobDto> UpdateJobStatusAsync(Guid tenantId, Guid id, UpdateServiceJobStatusDto dto, CancellationToken ct = default)
    {
        var job = await db.ServiceClientJobs.Include(j => j.Technician)
            .FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.ServiceClientJob), id);
        job.Status = dto.Status;
        await db.SaveChangesAsync(ct);
        return Map(job);
    }

    public async Task VoidJobAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var job = await db.ServiceClientJobs.FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.ServiceClientJob), id);
        job.Status = ServiceJobStatus.CANCELLED;
        await db.SaveChangesAsync(ct);
    }

    private static ServiceClientJobDto Map(Domain.Entities.ServiceClientJob j) => new(
        j.Id, j.SvCode, j.RetailClientName, j.RetailClientPhone, j.DeviceModel,
        j.ImeiSerial, j.IssueDescription, j.Technician?.Name, j.PriceToClientUsd,
        j.TechnicianCostUsd, j.DepositAmount, j.Status, j.LimitDate, j.CreatedAt
    );
}

// ─── CUENTAS CORRIENTES SERVICE ────────────────────────────
public class CuentasCorrientesService(AppDbContext db) : ICuentasCorrientesService
{
    public async Task<PagedResult<EntityBalanceDto>> GetBalancesAsync(Guid tenantId, EntityType? type, string? filter, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.Entities.Include(e => e.Balance)
            .Where(e => e.TenantId == tenantId && e.IsActive);
        if (type.HasValue) q = q.Where(e => e.Type == type.Value);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            q = filter switch
            {
                "cobrar" => q.Where(e => e.Balance != null && e.Balance.BalanceUsd > 0),
                "pagar" => q.Where(e => e.Balance != null && e.Balance.BalanceUsd < 0),
                "cero" => q.Where(e => e.Balance == null || e.Balance.BalanceUsd == 0),
                _ => q
            };
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(e => e.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new EntityBalanceDto(e.Id, e.Name, e.Phone, e.Type, e.Balance != null ? e.Balance.BalanceUsd : 0))
            .ToListAsync(ct);
        return new PagedResult<EntityBalanceDto>(items, total, page, pageSize);
    }

    public async Task<EntityBalanceDto?> GetByEntityAsync(Guid tenantId, Guid entityId, CancellationToken ct = default)
    {
        var e = await db.Entities.Include(x => x.Balance)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entityId, ct);
        if (e == null) return null;
        return new EntityBalanceDto(e.Id, e.Name, e.Phone, e.Type, e.Balance?.BalanceUsd ?? 0);
    }

    public async Task RecordPaymentAsync(Guid tenantId, RecordDebtPaymentDto dto, CancellationToken ct = default)
    {
        var bal = await db.EntityBalances.FindAsync([dto.EntityId], ct);
        if (bal == null) throw new NotFoundException(nameof(Domain.Entities.EntityBalance), dto.EntityId);
        bal.BalanceUsd += dto.AmountUsd;
        bal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

// ─── RETENTION SERVICE ─────────────────────────────────────
public class RetentionService(AppDbContext db) : IRetentionService
{
    public async Task<IEnumerable<RetentionRuleDto>> GetRulesAsync(Guid tenantId, CancellationToken ct = default)
        => await db.RetentionRules.Where(r => r.TenantId == tenantId)
            .Select(r => new RetentionRuleDto(r.Id, r.RuleType, r.DaysAfterSale, r.MessageTemplate, r.IsActive))
            .ToListAsync(ct);

    public async Task<RetentionRuleDto> UpsertRuleAsync(Guid tenantId, RetentionRuleDto dto, CancellationToken ct = default)
    {
        var rule = await db.RetentionRules.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == dto.Id, ct);
        if (rule == null)
        {
            rule = new Domain.Entities.RetentionRule { TenantId = tenantId };
            db.RetentionRules.Add(rule);
        }
        rule.RuleType = dto.RuleType; rule.DaysAfterSale = dto.DaysAfterSale;
        rule.MessageTemplate = dto.MessageTemplate; rule.IsActive = dto.IsActive;
        await db.SaveChangesAsync(ct);
        return new RetentionRuleDto(rule.Id, rule.RuleType, rule.DaysAfterSale, rule.MessageTemplate, rule.IsActive);
    }

    public async Task<IEnumerable<RetentionTouchpointDto>> GetTouchpointsAsync(Guid tenantId, string? status, CancellationToken ct = default)
    {
        var rules = await db.RetentionRules.Where(r => r.TenantId == tenantId && r.IsActive).ToListAsync(ct);
        var sales = await db.Sales
            .Include(s => s.Entity)
            .Include(s => s.Items).ThenInclude(i => i.StockItem).ThenInclude(si => si!.Model)
            .Where(s => s.TenantId == tenantId && s.Status == SaleStatus.COMPLETED)
            .ToListAsync(ct);
        var now = DateTime.UtcNow;
        var touchpoints = new List<RetentionTouchpointDto>();
        foreach (var sale in sales)
            foreach (var rule in rules)
            {
                var triggerDate = sale.SaleDate.AddDays(rule.DaysAfterSale);
                var tpStatus = triggerDate.Date == now.Date ? "PARA_HOY" :
                               triggerDate < now ? "VENCIDO" : "PENDIENTE";
                if (status != null && tpStatus != status) continue;

                var clientName = sale.Entity?.Name ?? sale.RetailClientName ?? "Cliente";
                var modelName = sale.Items
                    .Where(i => i.StockItem?.Model != null)
                    .Select(i => i.StockItem!.Model.Name)
                    .FirstOrDefault() ?? "tu equipo";
                var message = rule.MessageTemplate
                    .Replace("{cliente}", clientName)
                    .Replace("{modelo}", modelName);

                touchpoints.Add(new RetentionTouchpointDto(
                    sale.Id, clientName,
                    sale.Entity?.Phone ?? sale.RetailClientPhone,
                    rule.RuleType, message, triggerDate, tpStatus));
            }
        return touchpoints.OrderBy(t => t.TriggerDate);
    }
}

// ─── CATALOG SERVICE ───────────────────────────────────────
public class CatalogService(AppDbContext db) : ICatalogService
{
    public async Task<IEnumerable<CatalogModelDto>> GetModelsAsync(Guid tenantId, CancellationToken ct = default)
        => await db.CatalogModels.Where(m => m.TenantId == tenantId)
            .Select(m => new CatalogModelDto(m.Id, m.Name, m.IdType, m.RequiresStorage, m.RequiresColor))
            .ToListAsync(ct);

    public async Task<IEnumerable<CatalogAccessoryDto>> GetAccessoriesAsync(Guid tenantId, CancellationToken ct = default)
        => await db.CatalogAccessories.Where(a => a.TenantId == tenantId)
            .Select(a => new CatalogAccessoryDto(a.Id, a.Name, a.RequiresModel, a.RequiresColor))
            .ToListAsync(ct);

    public async Task<IEnumerable<CatalogLocationDto>> GetLocationsAsync(Guid tenantId, CancellationToken ct = default)
        => await db.CatalogLocations.Where(l => l.TenantId == tenantId)
            .Select(l => new CatalogLocationDto(l.Id, l.Name))
            .ToListAsync(ct);

    public async Task<CatalogModelDto> CreateModelAsync(Guid tenantId, string name, string idType, CancellationToken ct = default)
    {
        var m = new Domain.Entities.CatalogModel { TenantId = tenantId, Name = name, IdType = idType };
        db.CatalogModels.Add(m);
        await db.SaveChangesAsync(ct);
        return new CatalogModelDto(m.Id, m.Name, m.IdType, m.RequiresStorage, m.RequiresColor);
    }

    public async Task<CatalogAccessoryDto> CreateAccessoryAsync(Guid tenantId, string name, CancellationToken ct = default)
    {
        var a = new Domain.Entities.CatalogAccessory { TenantId = tenantId, Name = name };
        db.CatalogAccessories.Add(a);
        await db.SaveChangesAsync(ct);
        return new CatalogAccessoryDto(a.Id, a.Name, a.RequiresModel, a.RequiresColor);
    }

    public async Task<CatalogLocationDto> CreateLocationAsync(Guid tenantId, string name, CancellationToken ct = default)
    {
        var l = new Domain.Entities.CatalogLocation { TenantId = tenantId, Name = name };
        db.CatalogLocations.Add(l);
        await db.SaveChangesAsync(ct);
        return new CatalogLocationDto(l.Id, l.Name);
    }
}

// ─── TC BLUE SERVICE ───────────────────────────────────────
public class TcBlueService : ITcBlueService
{
    public async Task<decimal> GetCurrentRateAsync(CancellationToken ct = default)
    {
        try
        {
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://api.bluelytics.com.ar/v2/latest", ct);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("blue").GetProperty("value_sell").GetDecimal();
        }
        catch
        {
            return 1520m; // fallback si la API no responde
        }
    }
}

public class AgendaService(AppDbContext db) : IAgendaService
{
    public async Task<IEnumerable<CalendarEventDto>> GetByMonthAsync(Guid tenantId, int year, int month, CancellationToken ct = default)
    {
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);
        return await db.CalendarEvents
          .Where(e => e.TenantId == tenantId && e.StartTime >= from && e.StartTime < to)
          .OrderBy(e => e.StartTime)
          .Select(e => new CalendarEventDto(e.Id, e.Title, e.Description, e.StartTime, e.EndTime, e.Type))
          .ToListAsync(ct);
    }

    public async Task<CalendarEventDto> CreateAsync(Guid tenantId, CreateCalendarEventDto dto, CancellationToken ct = default)
    {
        var ev = new Domain.Entities.CalendarEvent
        {
            TenantId = tenantId,
            Title = dto.Title,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Type = dto.Type
        };
        db.CalendarEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        return new CalendarEventDto(ev.Id, ev.Title, ev.Description, ev.StartTime, ev.EndTime, ev.Type);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var ev = await db.CalendarEvents.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct)
          ?? throw new NotFoundException(nameof(Domain.Entities.CalendarEvent), id);
        db.CalendarEvents.Remove(ev);
        await db.SaveChangesAsync(ct);
    }
}

// ─── AUTH SERVICE ──────────────────────────────────────────
public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<AuthUserDto?> ValidateAsync(string email, string password, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.TenantUsers
            .FirstOrDefaultAsync(u => u.Email == normalized && u.IsActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return ToDto(user);
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
    {
        var email = dto.OwnerEmail.Trim().ToLowerInvariant();

        if (await db.TenantUsers.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Ya existe una cuenta con ese email.");

        var tenant = new Domain.Entities.Tenant
        {
            Name = dto.BusinessName.Trim(),
            City = string.Empty,
            Country = "AR",
            Plan = Domain.Enums.TenantPlan.STARTER,
        };
        db.Tenants.Add(tenant);

        var user = new Domain.Entities.TenantUser
        {
            TenantId = tenant.Id,
            UserId = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.OwnerPassword),
            DisplayName = dto.OwnerDisplayName.Trim(),
            InvitedEmail = email,
            Role = Domain.Enums.UserRole.OWNER,
            IsActive = true,
        };
        db.TenantUsers.Add(user);

        await db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<AuthUserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.TenantUsers.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        return user is null ? null : ToDto(user);
    }

    public string GenerateToken(AuthUserDto user)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("userId", user.UserId.ToString()),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role),
            new Claim(ClaimTypes.Name, user.DisplayName),
        };
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(config["Jwt:ExpiryDays"] ?? "7")),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthUserDto ToDto(Domain.Entities.TenantUser u) =>
        new(u.Id, u.TenantId, u.Email, u.DisplayName, u.Role.ToString());
}

// ─── INVITATION SERVICE ────────────────────────────────────
public class InvitationService(AppDbContext db, IEmailService emailService, IConfiguration config, ILogger<InvitationService> logger) : IInvitationService
{
    public async Task<InvitationLinkDto> CreateAsync(Guid tenantId, Guid invitedByUserId, string email, string baseUrl, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        if (await db.TenantUsers.AnyAsync(u => u.Email == normalized && u.IsActive, ct))
            throw new ConflictException("Ya existe un usuario activo con ese email.");

        // Drop any previous pending invite for the same email in this tenant
        var stale = await db.TenantInvitations
            .Where(i => i.TenantId == tenantId && i.Email == normalized && i.Status == Domain.Enums.InvitationStatus.PENDING)
            .ToListAsync(ct);
        db.TenantInvitations.RemoveRange(stale);

        var inv = new Domain.Entities.TenantInvitation
        {
            TenantId = tenantId,
            Email = normalized,
            Token = Guid.NewGuid(),
            InvitedByUserId = invitedByUserId,
            Status = Domain.Enums.InvitationStatus.PENDING,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };
        db.TenantInvitations.Add(inv);
        await db.SaveChangesAsync(ct);

        var acceptUrl = $"{baseUrl}/aceptar-invitacion?token={inv.Token}";

        // Best-effort notification email — never blocks or breaks invitation creation.
        try
        {
            var tenant = await db.Tenants.FindAsync([tenantId], ct);
            var emailBaseUrl = config["Frontend:BaseUrl"] ?? baseUrl;
            var invitationLink = $"{emailBaseUrl}/aceptar-invitacion?token={inv.Token}";
            await emailService.SendInvitationAsync(inv.Email, tenant?.Name ?? "iP Gestión", invitationLink, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo enviar el email de invitación a {Email}.", inv.Email);
        }

        return new InvitationLinkDto(inv.Id, inv.Email, inv.Token.ToString(), acceptUrl, inv.ExpiresAt);
    }

    public async Task<InvitationInfoDto?> ValidateTokenAsync(Guid token, CancellationToken ct = default)
    {
        var inv = await db.TenantInvitations
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.Token == token, ct);

        if (inv is null
            || inv.Status != Domain.Enums.InvitationStatus.PENDING
            || inv.ExpiresAt < DateTime.UtcNow)
            return null;

        return new InvitationInfoDto(inv.Email, inv.Tenant.Name);
    }

    public async Task<AuthUserDto> AcceptAsync(AcceptInvitationDto dto, CancellationToken ct = default)
    {
        if (!Guid.TryParse(dto.Token, out var token))
            throw new ConflictException("Token de invitación inválido.");

        var inv = await db.TenantInvitations.FirstOrDefaultAsync(i => i.Token == token, ct)
            ?? throw new ConflictException("La invitación no existe.");

        if (inv.Status != Domain.Enums.InvitationStatus.PENDING)
            throw new ConflictException("Esta invitación ya fue utilizada.");
        if (inv.ExpiresAt < DateTime.UtcNow)
        {
            inv.Status = Domain.Enums.InvitationStatus.EXPIRED;
            await db.SaveChangesAsync(ct);
            throw new ConflictException("La invitación expiró.");
        }

        if (await db.TenantUsers.AnyAsync(u => u.Email == inv.Email && u.IsActive, ct))
            throw new ConflictException("Ya existe un usuario activo con ese email.");

        var user = new Domain.Entities.TenantUser
        {
            TenantId = inv.TenantId,
            UserId = Guid.NewGuid(),
            Email = inv.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            DisplayName = dto.DisplayName.Trim(),
            InvitedEmail = inv.Email,
            Role = Domain.Enums.UserRole.EMPLOYEE,
            IsActive = true,
        };
        db.TenantUsers.Add(user);
        inv.Status = Domain.Enums.InvitationStatus.ACCEPTED;
        await db.SaveChangesAsync(ct);

        return new AuthUserDto(user.Id, user.TenantId, user.Email, user.DisplayName, user.Role.ToString());
    }

    public async Task<IEnumerable<PendingInvitationDto>> GetPendingAsync(Guid tenantId, CancellationToken ct = default)
        => await db.TenantInvitations
            .Where(i => i.TenantId == tenantId && i.Status == Domain.Enums.InvitationStatus.PENDING && i.ExpiresAt >= DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new PendingInvitationDto(i.Id, i.Email, i.Token.ToString(), i.CreatedAt, i.ExpiresAt))
            .ToListAsync(ct);

    public async Task CancelAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var inv = await db.TenantInvitations.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == id, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.TenantInvitation), id);
        db.TenantInvitations.Remove(inv);
        await db.SaveChangesAsync(ct);
    }
}

// ─── USER (TEAM) SERVICE ───────────────────────────────────
public class UserService(AppDbContext db) : IUserService
{
    public async Task<IEnumerable<TenantUserDto>> GetUsersAsync(Guid tenantId, CancellationToken ct = default)
        => await db.TenantUsers
            .Where(u => u.TenantId == tenantId && u.IsActive)
            .OrderBy(u => u.Role == Domain.Enums.UserRole.OWNER ? 0 : 1)
            .ThenBy(u => u.DisplayName)
            .Select(u => new TenantUserDto(u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToListAsync(ct);

    public async Task DeactivateAsync(Guid tenantId, Guid userId, Guid currentUserId, CancellationToken ct = default)
    {
        if (userId == currentUserId)
            throw new ConflictException("No podés desactivarte a vos mismo.");

        var user = await db.TenantUsers.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.TenantUser), userId);

        if (user.Role == Domain.Enums.UserRole.OWNER)
            throw new ConflictException("No se puede desactivar al dueño de la cuenta.");

        user.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}
