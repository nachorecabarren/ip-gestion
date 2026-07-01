using IpGestion.Application.Common.DTOs;
using IpGestion.Domain.Enums;

namespace IpGestion.Application.Interfaces;

public interface IAuthService
{
    Task<AuthUserDto?> ValidateAsync(string email, string password, CancellationToken ct = default);
    Task<AuthUserDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<AuthUserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    string GenerateToken(AuthUserDto user);
}

public interface IInvitationService
{
    Task<InvitationLinkDto> CreateAsync(Guid tenantId, Guid invitedByUserId, string email, string baseUrl, CancellationToken ct = default);
    Task<InvitationInfoDto?> ValidateTokenAsync(Guid token, CancellationToken ct = default);
    Task<AuthUserDto> AcceptAsync(AcceptInvitationDto dto, CancellationToken ct = default);
    Task<IEnumerable<PendingInvitationDto>> GetPendingAsync(Guid tenantId, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IUserService
{
    Task<IEnumerable<TenantUserDto>> GetUsersAsync(Guid tenantId, CancellationToken ct = default);
    Task DeactivateAsync(Guid tenantId, Guid userId, Guid currentUserId, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardKpisDto> GetKpisAsync(Guid tenantId, string periodo, CancellationToken ct = default);
    Task<IEnumerable<QuickSaleDto>> GetRecentSalesAsync(Guid tenantId, int count = 10, CancellationToken ct = default);
}

public interface IEntityService
{
    Task<PagedResult<EntityDto>> GetPagedAsync(Guid tenantId, EntityType? type, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<EntityDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<EntityDto> CreateAsync(Guid tenantId, CreateEntityDto dto, CancellationToken ct = default);
    Task<EntityDto> UpdateAsync(Guid tenantId, Guid id, UpdateEntityDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IStockService
{
    Task<PagedResult<StockItemDto>> GetItemsPagedAsync(Guid tenantId, StockStatus? status, StockCondition? condition, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<StockItemDto?> GetItemByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<StockItemDto?> GetByBarcodeAsync(Guid tenantId, string barcode, CancellationToken ct = default);
    Task<StockItemDto> CreateItemAsync(Guid tenantId, CreateStockItemDto dto, CancellationToken ct = default);
    Task<StockItemDto> UpdateItemAsync(Guid tenantId, Guid id, UpdateStockItemDto dto, CancellationToken ct = default);
    Task VoidItemAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<StockBulkDto>> GetBulkItemsAsync(Guid tenantId, CancellationToken ct = default);
    Task<TradeInQuoteDto> GetTradeInQuoteAsync(Guid tenantId, TradeInQuoteRequestDto dto, CancellationToken ct = default);
    Task BulkUpdatePricesAsync(Guid tenantId, List<Guid> itemIds, decimal newPrice, CancellationToken ct = default);
    Task TransferStockAsync(Guid tenantId, List<Guid> itemIds, Guid targetLocationId, CancellationToken ct = default);
}

public interface ISaleService
{
    Task<PagedResult<SaleDto>> GetPagedAsync(Guid tenantId, SaleCategory? category, SaleOrigin? origin, string? search, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default);
    Task<SaleDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<SaleDto> CreateAsync(Guid tenantId, CreateSaleDto dto, CancellationToken ct = default);
    Task VoidSaleAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IPurchaseService
{
    Task<PagedResult<PurchaseDto>> GetPagedAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<PurchaseDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PurchaseDto> CreateAsync(Guid tenantId, CreatePurchaseDto dto, CancellationToken ct = default);
    Task VoidAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetPagedAsync(Guid tenantId, ReservationStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<ReservationDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ReservationDto> CreateAsync(Guid tenantId, CreateReservationDto dto, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<SaleDto> ConvertToSaleAsync(Guid tenantId, Guid reservationId, CreateSaleDto dto, CancellationToken ct = default);
}

public interface IImportService
{
    Task<PagedResult<ImportOrderDto>> GetPagedAsync(Guid tenantId, int page, int pageSize, CancellationToken ct = default);
    Task<ImportOrderDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ImportOrderDto> CreatePurchaseRequestAsync(Guid tenantId, CreateImportOrderDto dto, CancellationToken ct = default);
    Task ReceiveOrderAsync(Guid tenantId, Guid orderId, List<ReceiveImportItemDto> items, CancellationToken ct = default);
    Task CancelAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface ICajaService
{
    Task<IEnumerable<CajaDto>> GetCajasAsync(Guid tenantId, CancellationToken ct = default);
    Task<IEnumerable<CashMovementDto>> GetMovementsAsync(Guid tenantId, Guid? cajaId, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default);
    Task<CashMovementDto> RegisterMovementAsync(Guid tenantId, CreateCashMovementDto dto, CancellationToken ct = default);
    Task CloseDayAsync(Guid tenantId, DateOnly date, CancellationToken ct = default);
}

public interface IServiceTechService
{
    Task<PagedResult<ServiceClientJobDto>> GetClientJobsPagedAsync(Guid tenantId, ServiceJobStatus? status, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<ServiceClientJobDto?> GetClientJobByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ServiceClientJobDto> CreateClientJobAsync(Guid tenantId, CreateServiceClientJobDto dto, CancellationToken ct = default);
    Task<ServiceClientJobDto> UpdateJobStatusAsync(Guid tenantId, Guid id, UpdateServiceJobStatusDto dto, CancellationToken ct = default);
    Task VoidJobAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface ICuentasCorrientesService
{
    Task<PagedResult<EntityBalanceDto>> GetBalancesAsync(Guid tenantId, EntityType? type, string? filter, int page, int pageSize, CancellationToken ct = default);
    Task<EntityBalanceDto?> GetByEntityAsync(Guid tenantId, Guid entityId, CancellationToken ct = default);
    Task RecordPaymentAsync(Guid tenantId, RecordDebtPaymentDto dto, CancellationToken ct = default);
}

public interface IRetentionService
{
    Task<IEnumerable<RetentionRuleDto>> GetRulesAsync(Guid tenantId, CancellationToken ct = default);
    Task<RetentionRuleDto> UpsertRuleAsync(Guid tenantId, RetentionRuleDto dto, CancellationToken ct = default);
    Task<IEnumerable<RetentionTouchpointDto>> GetTouchpointsAsync(Guid tenantId, string? status, CancellationToken ct = default);
}

public interface IObjectionService
{
    Task<PagedResult<ObjectionDto>> GetPagedAsync(Guid tenantId, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<ObjectionDto> CreateAsync(Guid tenantId, CreateObjectionDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface ICompetitorService
{
    Task<IEnumerable<CompetitorDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<CompetitorDto> UpsertAsync(Guid tenantId, string competitorName, List<CompetitorPriceDto> prices, CancellationToken ct = default);
}

public interface IAgendaService
{
    Task<IEnumerable<CalendarEventDto>> GetByMonthAsync(Guid tenantId, int year, int month, CancellationToken ct = default);
    Task<CalendarEventDto> CreateAsync(Guid tenantId, CreateCalendarEventDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface ICatalogService
{
    Task<IEnumerable<CatalogModelDto>> GetModelsAsync(Guid tenantId, CancellationToken ct = default);
    Task<IEnumerable<CatalogAccessoryDto>> GetAccessoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<IEnumerable<CatalogLocationDto>> GetLocationsAsync(Guid tenantId, CancellationToken ct = default);
    Task<CatalogModelDto> CreateModelAsync(Guid tenantId, string name, string idType, CancellationToken ct = default);
    Task<CatalogAccessoryDto> CreateAccessoryAsync(Guid tenantId, string name, CancellationToken ct = default);
    Task<CatalogLocationDto> CreateLocationAsync(Guid tenantId, string name, CancellationToken ct = default);
}

public interface ITcBlueService
{
    Task<decimal> GetCurrentRateAsync(CancellationToken ct = default);
}
