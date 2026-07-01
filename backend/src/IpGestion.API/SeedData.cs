using IpGestion.Domain.Entities;
using IpGestion.Domain.Enums;
using IpGestion.Infrastructure.Persistence;

namespace IpGestion.API;

public static class SeedData
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Tenants.Any()) return;

        // Tenant
        var tenant = new Tenant {
            Id = TenantId, Name = "iP Argentina", City = "Buenos Aires", Country = "AR",
            Plan = TenantPlan.PRO, WholesalePriceEnabled = true, CloserCommissionEnabled = true,
            CloserCommissionType = CommissionType.FIXED, CloserCommissionValue = 10
        };
        db.Tenants.Add(tenant);

        // Owner login user — email/password to sign in right after the first run.
        db.TenantUsers.Add(new TenantUser {
            TenantId = TenantId,
            UserId = Guid.NewGuid(),
            Email = "admin@ipgestion.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            DisplayName = "Administrador",
            InvitedEmail = "admin@ipgestion.com",
            Role = UserRole.OWNER,
            IsActive = true
        });

        // Catalog Models
        var models = new[] {
            new CatalogModel { TenantId = TenantId, Name = "iPhone 16 Pro Max", IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 16 Pro",     IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 16",         IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 15 Pro Max", IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 15 Pro",     IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 15",         IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 14 Pro Max", IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
            new CatalogModel { TenantId = TenantId, Name = "iPhone 14",         IdType = "IMEI", RequiresStorage = true, RequiresColor = true },
        };
        db.CatalogModels.AddRange(models);

        // Catalog Accessories
        var acc1 = new CatalogAccessory { TenantId = TenantId, Name = "Funda iPhone 16 Pro Max", RequiresModel = true, RequiresColor = true };
        var acc2 = new CatalogAccessory { TenantId = TenantId, Name = "Vidrio Templado", RequiresModel = true, RequiresColor = false };
        var acc3 = new CatalogAccessory { TenantId = TenantId, Name = "Cable USB-C", RequiresModel = false };
        var acc4 = new CatalogAccessory { TenantId = TenantId, Name = "Auriculares AirPods Pro 2", RequiresModel = false };
        db.CatalogAccessories.AddRange(acc1, acc2, acc3, acc4);

        // Locations
        var loc1 = new CatalogLocation { TenantId = TenantId, Name = "Depósito Principal" };
        var loc2 = new CatalogLocation { TenantId = TenantId, Name = "Vitrina" };
        var loc3 = new CatalogLocation { TenantId = TenantId, Name = "Caja Fuerte" };
        db.CatalogLocations.AddRange(loc1, loc2, loc3);

        // Entities (Clients)
        var cli1 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.CLIENT, Name = "Martín González", Phone = "+54 9 11 4523-7890", Instagram = "@martin.g", Email = "martin@gmail.com", AddressCity = "CABA", IsActive = true };
        var cli2 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.CLIENT, Name = "Laura Fernández", Phone = "+54 9 11 6712-4321", Instagram = "@laufern", Email = "laura.f@hotmail.com", AddressCity = "GBA Norte", IsActive = true };
        var cli3 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.CLIENT, Name = "Diego Pereyra", Phone = "+54 9 351 445-2233", AddressCity = "Córdoba", IsActive = true };
        var cli4 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.CLIENT, Name = "Valentina Castro", Phone = "+54 9 261 334-5567", Email = "vale.castro@gmail.com", IsActive = true };

        // Entities (Providers)
        var prov1 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.PROVIDER, Name = "AppleZone USA", Phone = "+1 305 555-0100", Email = "ventas@applezone.com", AddressCity = "Miami", IsActive = true };
        var prov2 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.PROVIDER, Name = "TechImport Paraguay", Phone = "+595 21 555-8800", AddressCity = "Asunción", IsActive = true };

        // Entities (Technicians)
        var tech1 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.TECHNICIAN, Name = "Carlos Méndez", Phone = "+54 9 11 5234-9900", IsActive = true };

        // Entities (Couriers)
        var cour1 = new Domain.Entities.Entity { TenantId = TenantId, Type = EntityType.COURIER, Name = "OCA Express", Phone = "0810-999-0622", IsActive = true };

        db.Entities.AddRange(cli1, cli2, cli3, cli4, prov1, prov2, tech1, cour1);

        // Balances
        db.EntityBalances.AddRange(
            new EntityBalance { EntityId = cli1.Id, BalanceUsd = 150 },
            new EntityBalance { EntityId = cli2.Id, BalanceUsd = 0 },
            new EntityBalance { EntityId = cli3.Id, BalanceUsd = -80 },
            new EntityBalance { EntityId = cli4.Id, BalanceUsd = 0 },
            new EntityBalance { EntityId = prov1.Id, BalanceUsd = -2400 },
            new EntityBalance { EntityId = prov2.Id, BalanceUsd = 0 },
            new EntityBalance { EntityId = tech1.Id, BalanceUsd = 0 },
            new EntityBalance { EntityId = cour1.Id, BalanceUsd = 0 }
        );

        // Stock items
        var stock = new[] {
            new StockItem { TenantId = TenantId, ModelId = models[0].Id, InternalCode = "IP-00001", ImeiSerial = "356938035643809", Color = "Negro Titanio", StorageGb = 256, Condition = StockCondition.NEW, BatteryPct = 100, CostUsd = 820, SuggestedPriceUsd = 980, WholesalePriceUsd = 940, Status = StockStatus.AVAILABLE, LocationId = loc2.Id },
            new StockItem { TenantId = TenantId, ModelId = models[0].Id, InternalCode = "IP-00002", ImeiSerial = "356938035643810", Color = "Blanco Titanio", StorageGb = 512, Condition = StockCondition.NEW, BatteryPct = 100, CostUsd = 920, SuggestedPriceUsd = 1100, WholesalePriceUsd = 1060, Status = StockStatus.AVAILABLE, LocationId = loc2.Id },
            new StockItem { TenantId = TenantId, ModelId = models[1].Id, InternalCode = "IP-00003", ImeiSerial = "356938035643811", Color = "Titanio Desértico", StorageGb = 256, Condition = StockCondition.NEW, BatteryPct = 100, CostUsd = 750, SuggestedPriceUsd = 890, WholesalePriceUsd = 860, Status = StockStatus.AVAILABLE, LocationId = loc2.Id },
            new StockItem { TenantId = TenantId, ModelId = models[3].Id, InternalCode = "IP-00004", ImeiSerial = "356938035643812", Color = "Negro Espacial", StorageGb = 256, Condition = StockCondition.USED, ConditionGrade = "A", BatteryPct = 89, CostUsd = 560, SuggestedPriceUsd = 680, WholesalePriceUsd = 650, Status = StockStatus.AVAILABLE, LocationId = loc1.Id },
            new StockItem { TenantId = TenantId, ModelId = models[4].Id, InternalCode = "IP-00005", ImeiSerial = "356938035643813", Color = "Plata", StorageGb = 128, Condition = StockCondition.USED, ConditionGrade = "A+", BatteryPct = 94, CostUsd = 480, SuggestedPriceUsd = 580, WholesalePriceUsd = 550, Status = StockStatus.RESERVED, LocationId = loc1.Id },
            new StockItem { TenantId = TenantId, ModelId = models[2].Id, InternalCode = "IP-00006", ImeiSerial = "356938035643814", Color = "Ultramarino", StorageGb = 128, Condition = StockCondition.NEW, BatteryPct = 100, CostUsd = 620, SuggestedPriceUsd = 750, WholesalePriceUsd = 720, Status = StockStatus.AVAILABLE, LocationId = loc2.Id },
        };
        db.StockItems.AddRange(stock);

        // Stock Bulk
        db.StockBulks.AddRange(
            new StockBulk { TenantId = TenantId, AccessoryId = acc1.Id, Color = "Negro", Quantity = 12, LowStockThreshold = 3, CostUsd = 8, SuggestedPriceUsd = 18, LocationId = loc1.Id },
            new StockBulk { TenantId = TenantId, AccessoryId = acc2.Id, Quantity = 30, LowStockThreshold = 5, CostUsd = 3, SuggestedPriceUsd = 8, LocationId = loc1.Id },
            new StockBulk { TenantId = TenantId, AccessoryId = acc3.Id, Quantity = 20, LowStockThreshold = 5, CostUsd = 5, SuggestedPriceUsd = 12, LocationId = loc1.Id },
            new StockBulk { TenantId = TenantId, AccessoryId = acc4.Id, Quantity = 4, LowStockThreshold = 2, CostUsd = 180, SuggestedPriceUsd = 250, LocationId = loc3.Id }
        );

        // Caja
        var caja = new Caja { TenantId = TenantId, Name = "Caja Principal", IsDefault = true, IsActive = true };
        db.Cajas.Add(caja);

        // Movement categories
        var catOp = new MovementCategory { TenantId = TenantId, Name = "Gasto Operativo" };
        var catSal = new MovementCategory { TenantId = TenantId, Name = "Salarios" };
        var catAlq = new MovementCategory { TenantId = TenantId, Name = "Alquiler" };
        db.MovementCategories.AddRange(catOp, catSal, catAlq);

        // Sales históricos
        var sale1 = new Sale {
            TenantId = TenantId, EntityId = cli1.Id, SaleCategory = SaleCategory.RETAIL,
            Origin = SaleOrigin.DIRECT, TotalUsd = 980, WarrantyDays = 90, Status = SaleStatus.COMPLETED,
            SaleDate = DateTime.UtcNow.AddDays(-5),
            Items = [ new SaleItem { TenantId = TenantId, Type = ItemKind.EQUIPMENT, StockItemId = stock[0].Id, Quantity = 1, PriceUsd = 980 } ],
            Payments = [ new TransactionPayment { TenantId = TenantId, ReferenceType = "SALE", Method = PaymentMethod.USD_CASH, Currency = Currency.USD, Amount = 980, AmountUsd = 980, ExchangeRateUsd = 1 } ]
        };
        stock[0].Status = StockStatus.SOLD;

        var sale2 = new Sale {
            TenantId = TenantId, EntityId = cli2.Id, SaleCategory = SaleCategory.WHOLESALE,
            Origin = SaleOrigin.DIRECT, TotalUsd = 860, WarrantyDays = 30, Status = SaleStatus.COMPLETED,
            SaleDate = DateTime.UtcNow.AddDays(-3),
            Items = [ new SaleItem { TenantId = TenantId, Type = ItemKind.EQUIPMENT, StockItemId = stock[2].Id, Quantity = 1, PriceUsd = 860 } ],
            Payments = [ new TransactionPayment { TenantId = TenantId, ReferenceType = "SALE", Method = PaymentMethod.USDT, Currency = Currency.USD, Amount = 860, AmountUsd = 860, ExchangeRateUsd = 1 } ]
        };
        stock[2].Status = StockStatus.SOLD;

        db.Sales.AddRange(sale1, sale2);

        // Reservation
        db.Reservations.Add(new Reservation {
            TenantId = TenantId, EntityId = cli3.Id, StockItemId = stock[4].Id,
            SaleCategory = SaleCategory.RETAIL, PickupDate = DateTime.UtcNow.AddDays(3),
            AgreedPriceUsd = 580, DepositAmountUsd = 100, Status = ReservationStatus.ACTIVE,
            Notes = "Retira el jueves a la tarde"
        });

        // Service jobs
        db.ServiceClientJobs.AddRange(
            new ServiceClientJob {
                TenantId = TenantId, SvCode = "SV-001", RetailClientName = "Roberto Silva",
                RetailClientPhone = "+54 9 11 3344-5566", DeviceModel = "iPhone 14 Pro",
                ImeiSerial = "356938035643899", IssueDescription = "Pantalla rota, táctil sin respuesta",
                TechnicianId = tech1.Id, PriceToClientUsd = 120, TechnicianCostUsd = 80,
                DepositMethod = PaymentMethod.ARS_CASH, DepositAmount = 50,
                Status = ServiceJobStatus.IN_REPAIR, LimitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5))
            },
            new ServiceClientJob {
                TenantId = TenantId, SvCode = "SV-002", RetailClientName = "Ana Rodríguez",
                RetailClientPhone = "+54 9 11 7788-9900", DeviceModel = "iPhone 15",
                IssueDescription = "No carga, puerto dañado",
                TechnicianId = tech1.Id, PriceToClientUsd = 60, TechnicianCostUsd = 35,
                Status = ServiceJobStatus.READY_FOR_DELIVERY
            }
        );

        // Cash movements
        db.CashMovements.AddRange(
            new CashMovement { TenantId = TenantId, CajaId = caja.Id, Type = CashMovementType.SALE, Method = PaymentMethod.USD_CASH, Currency = Currency.USD, Amount = 980, AmountUsd = 980, ExchangeRateUsd = 1, Detail = "Venta iPhone 16 Pro Max", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new CashMovement { TenantId = TenantId, CajaId = caja.Id, Type = CashMovementType.SALE, Method = PaymentMethod.USDT, Currency = Currency.USD, Amount = 860, AmountUsd = 860, ExchangeRateUsd = 1, Detail = "Venta iPhone 16 Pro", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new CashMovement { TenantId = TenantId, CajaId = caja.Id, Type = CashMovementType.EXPENSE, Method = PaymentMethod.ARS_TR, Currency = Currency.ARS, Amount = 150000, AmountUsd = 98.68m, ExchangeRateUsd = 1520, MovementCategoryId = catAlq.Id, Detail = "Alquiler mensual", CreatedAt = DateTime.UtcNow.AddDays(-10) }
        );

        // Retention rules
        db.RetentionRules.AddRange(
            new RetentionRule { TenantId = TenantId, RuleType = "AURICULARES", DaysAfterSale = 90, MessageTemplate = "Hola {cliente}! Han pasado 90 días desde tu compra. ¿Te interesa ver los nuevos AirPods Pro?", IsActive = true },
            new RetentionRule { TenantId = TenantId, RuleType = "CHECK_IN", DaysAfterSale = 60, MessageTemplate = "Hola {cliente}! ¿Cómo estás? ¿Todo bien con tu {modelo}? Estamos acá para cualquier consulta.", IsActive = true },
            new RetentionRule { TenantId = TenantId, RuleType = "CAMBIO_TELEFONO", DaysAfterSale = 240, MessageTemplate = "Hola {cliente}! Ya pasaron 8 meses. ¿Pensás en renovar tu {modelo}? Tenemos los mejores precios del mercado.", IsActive = true }
        );

        // Competitors
        var comp1 = new Competitor { TenantId = TenantId, Name = "iShop Palermo", IsActive = true };
        var comp2 = new Competitor { TenantId = TenantId, Name = "CelTech Online", IsActive = true };
        db.Competitors.AddRange(comp1, comp2);
        db.CompetitorPrices.AddRange(
            new CompetitorPrice { TenantId = TenantId, CompetitorId = comp1.Id, ModelName = "iPhone 16 Pro Max 256GB", PriceUsd = 1010 },
            new CompetitorPrice { TenantId = TenantId, CompetitorId = comp1.Id, ModelName = "iPhone 16 Pro 256GB", PriceUsd = 910 },
            new CompetitorPrice { TenantId = TenantId, CompetitorId = comp2.Id, ModelName = "iPhone 16 Pro Max 256GB", PriceUsd = 995 },
            new CompetitorPrice { TenantId = TenantId, CompetitorId = comp2.Id, ModelName = "iPhone 15 Pro Max 256GB", PriceUsd = 720 }
        );

        // Trade-in valuations
        db.TradeInValuations.AddRange(
            new TradeInBaseValuation { TenantId = TenantId, ModelName = "iPhone 15 Pro Max", StorageGb = 256, BatteryRange = "80-100", BaseValueUsd = 550 },
            new TradeInBaseValuation { TenantId = TenantId, ModelName = "iPhone 15 Pro", StorageGb = 256, BatteryRange = "80-100", BaseValueUsd = 480 },
            new TradeInBaseValuation { TenantId = TenantId, ModelName = "iPhone 14 Pro Max", StorageGb = 256, BatteryRange = "80-100", BaseValueUsd = 420 },
            new TradeInBaseValuation { TenantId = TenantId, ModelName = "iPhone 14", StorageGb = 128, BatteryRange = "80-100", BaseValueUsd = 280 }
        );

        // Calendar events
        db.CalendarEvents.AddRange(
            new CalendarEvent { TenantId = TenantId, Title = "Retiro reserva - Diego Pereyra", StartTime = DateTime.UtcNow.AddDays(3).Date.AddHours(17), EndTime = DateTime.UtcNow.AddDays(3).Date.AddHours(17.5), Type = "RESERVATION" },
            new CalendarEvent { TenantId = TenantId, Title = "Reunión con proveedor AppleZone", StartTime = DateTime.UtcNow.AddDays(7).Date.AddHours(10), EndTime = DateTime.UtcNow.AddDays(7).Date.AddHours(11), Type = "MEETING" }
        );

        await db.SaveChangesAsync();
    }
}
