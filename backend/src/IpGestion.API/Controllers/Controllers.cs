using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IpGestion.Application.Common.DTOs;
using IpGestion.Application.Interfaces;
using IpGestion.Domain.Enums;

namespace IpGestion.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class TenantBaseController : ControllerBase
{
    // All identity comes from the JWT (read from the HttpOnly "jwt" cookie).
    protected Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue("userId")!);
    protected string UserRole => User.FindFirstValue("role") ?? string.Empty;
    protected bool IsOwner => UserRole == nameof(Domain.Enums.UserRole.OWNER);
}

// ─── DASHBOARD ─────────────────────────────────────────────
[Route("api/dashboard")]
public class DashboardController(IDashboardService svc) : TenantBaseController
{
    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis([FromQuery] string periodo = "month", CancellationToken ct = default)
        => Ok(await svc.GetKpisAsync(TenantId, periodo, ct));

    [HttpGet("recent-sales")]
    public async Task<IActionResult> GetRecentSales([FromQuery] int count = 10, CancellationToken ct = default)
        => Ok(await svc.GetRecentSalesAsync(TenantId, count, ct));
}

// ─── ENTITIES ──────────────────────────────────────────────
[Route("api/entidades")]
public class EntidadesController(IEntityService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EntityType? type, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(TenantId, type, search, page, pageSize, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await svc.GetByIdAsync(TenantId, id, ct);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEntityDto dto, CancellationToken ct = default)
    {
        var result = await svc.CreateAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEntityDto dto, CancellationToken ct = default)
        => Ok(await svc.UpdateAsync(TenantId, id, dto, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.DeleteAsync(TenantId, id, ct);
        return NoContent();
    }
}

// ─── STOCK ─────────────────────────────────────────────────
[Route("api/stock")]
public class StockController(IStockService svc) : TenantBaseController
{
    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] StockStatus? status, [FromQuery] StockCondition? condition,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetItemsPagedAsync(TenantId, status, condition, search, page, pageSize, ct));

    [HttpGet("items/{id:guid}")]
    public async Task<IActionResult> GetItem(Guid id, CancellationToken ct = default)
    {
        var r = await svc.GetItemByIdAsync(TenantId, id, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpGet("items/barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode(string barcode, CancellationToken ct = default)
    {
        var r = await svc.GetByBarcodeAsync(TenantId, barcode, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateStockItemDto dto, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.CreateItemAsync(TenantId, dto, ct));
    }

    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateStockItemDto dto, CancellationToken ct = default)
        => Ok(await svc.UpdateItemAsync(TenantId, id, dto, ct));

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> VoidItem(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.VoidItemAsync(TenantId, id, ct);
        return NoContent();
    }

    [HttpGet("bulk")]
    public async Task<IActionResult> GetBulk(CancellationToken ct = default)
        => Ok(await svc.GetBulkItemsAsync(TenantId, ct));

    [HttpPost("tradein/quote")]
    public async Task<IActionResult> GetTradeInQuote([FromBody] TradeInQuoteRequestDto dto, CancellationToken ct = default)
        => Ok(await svc.GetTradeInQuoteAsync(TenantId, dto, ct));

    [HttpPost("items/bulk-price")]
    public async Task<IActionResult> BulkUpdatePrices([FromBody] BulkPriceUpdateRequest req, CancellationToken ct = default)
    {
        await svc.BulkUpdatePricesAsync(TenantId, req.ItemIds, req.NewPrice, ct);
        return NoContent();
    }

    [HttpPost("items/transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest req, CancellationToken ct = default)
    {
        await svc.TransferStockAsync(TenantId, req.ItemIds, req.TargetLocationId, ct);
        return NoContent();
    }
}

public record BulkPriceUpdateRequest(List<Guid> ItemIds, decimal NewPrice);
public record TransferRequest(List<Guid> ItemIds, Guid TargetLocationId);

// ─── VENTAS ────────────────────────────────────────────────
[Route("api/ventas")]
public class VentasController(ISaleService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SaleCategory? category, [FromQuery] SaleOrigin? origin,
    [FromQuery] string? search, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
    [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    => Ok(await svc.GetPagedAsync(TenantId, category, origin, search, from, to, page, pageSize, ct));
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var r = await svc.GetByIdAsync(TenantId, id, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleDto dto, CancellationToken ct = default)
    {
        // Always include the current user so the sale shows who finalized it
        var closerIds = dto.CloserIds.Contains(CurrentUserId)
            ? dto.CloserIds
            : [.. dto.CloserIds, CurrentUserId];
        var result = await svc.CreateAsync(TenantId, dto with { CloserIds = closerIds }, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Void(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.VoidSaleAsync(TenantId, id, ct);
        return NoContent();
    }
}

// ─── COMPRAS ───────────────────────────────────────────────
[Route("api/compras")]
public class ComprasController(IPurchaseService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(TenantId, page, pageSize, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var r = await svc.GetByIdAsync(TenantId, id, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseDto dto, CancellationToken ct = default)
    {
        var result = await svc.CreateAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Void(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.VoidAsync(TenantId, id, ct);
        return NoContent();
    }
}

// ─── RESERVAS ──────────────────────────────────────────────
[Route("api/reservas")]
public class ReservasController(IReservationService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetPagedAsync(TenantId, status, page, pageSize, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var r = await svc.GetByIdAsync(TenantId, id, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto, CancellationToken ct = default)
    {
        var result = await svc.CreateAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.CancelAsync(TenantId, id, ct);
        return NoContent();
    }

    [HttpPost("{id}/convertir")]
    public async Task<IActionResult> ConvertToSale(Guid id, [FromBody] CreateSaleDto dto, CancellationToken ct = default)
        => Ok(await svc.ConvertToSaleAsync(TenantId, id, dto, ct));
}

// ─── CAJAS ─────────────────────────────────────────────────
[Route("api/cajas")]
public class CajasController(ICajaService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetCajas(CancellationToken ct = default)
        => Ok(await svc.GetCajasAsync(TenantId, ct));

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovements([FromQuery] Guid? cajaId, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
        => Ok(await svc.GetMovementsAsync(TenantId, cajaId, from, to, page, pageSize, ct));

    [HttpPost("movimientos")]
    public async Task<IActionResult> RegisterMovement([FromBody] CreateCashMovementDto dto, CancellationToken ct = default)
        => Ok(await svc.RegisterMovementAsync(TenantId, dto, ct));

    [HttpPost("cierre")]
    public async Task<IActionResult> CloseDay([FromBody] CloseDayRequest req, CancellationToken ct = default)
    {
        await svc.CloseDayAsync(TenantId, req.Date, ct);
        return Ok();
    }
}

public record CloseDayRequest(DateOnly Date);

// ─── SERVICIO TÉCNICO ──────────────────────────────────────
[Route("api/servicio-tecnico")]
public class ServicioTecnicoController(IServiceTechService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ServiceJobStatus? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await svc.GetClientJobsPagedAsync(TenantId, status, search, page, pageSize, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var r = await svc.GetClientJobByIdAsync(TenantId, id, ct);
        return r == null ? NotFound() : Ok(r);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceClientJobDto dto, CancellationToken ct = default)
    {
        var result = await svc.CreateClientJobAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateServiceJobStatusDto dto, CancellationToken ct = default)
        => Ok(await svc.UpdateJobStatusAsync(TenantId, id, dto, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Void(Guid id, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.VoidJobAsync(TenantId, id, ct);
        return NoContent();
    }
}

// ─── CUENTAS CORRIENTES ────────────────────────────────────
[Route("api/cuentas-corrientes")]
public class CuentasCorrientesController(ICuentasCorrientesService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EntityType? type, [FromQuery] string? filter,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        // Provider current accounts are sensitive — owner only.
        if (!IsOwner)
        {
            if (type == EntityType.PROVIDER) return Forbid();
            // Force a non-provider scope so the aggregate list never leaks providers.
            type ??= EntityType.CLIENT;
        }
        return Ok(await svc.GetBalancesAsync(TenantId, type, filter, page, pageSize, ct));
    }

    [HttpPost("pago")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordDebtPaymentDto dto, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        await svc.RecordPaymentAsync(TenantId, dto, ct);
        return Ok();
    }
}

// ─── RETENCIÓN ─────────────────────────────────────────────
[Route("api/retencion")]
public class RetencionController(IRetentionService svc) : TenantBaseController
{
    [HttpGet("reglas")]
    public async Task<IActionResult> GetRules(CancellationToken ct = default)
        => Ok(await svc.GetRulesAsync(TenantId, ct));

    [HttpPut("reglas")]
    public async Task<IActionResult> UpsertRule([FromBody] RetentionRuleDto dto, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.UpsertRuleAsync(TenantId, dto, ct));
    }

    [HttpGet("touchpoints")]
    public async Task<IActionResult> GetTouchpoints([FromQuery] string? status, CancellationToken ct = default)
        => Ok(await svc.GetTouchpointsAsync(TenantId, status, ct));
}

// ─── CATÁLOGOS ─────────────────────────────────────────────
[Route("api/catalogos")]
public class CatalogosController(ICatalogService svc) : TenantBaseController
{
    [HttpGet("modelos")]
    public async Task<IActionResult> GetModels(CancellationToken ct = default)
        => Ok(await svc.GetModelsAsync(TenantId, ct));

    [HttpPost("modelos")]
    public async Task<IActionResult> CreateModel([FromBody] CreateCatalogModelRequest req, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.CreateModelAsync(TenantId, req.Name, req.IdType, ct));
    }

    [HttpGet("accesorios")]
    public async Task<IActionResult> GetAccessories(CancellationToken ct = default)
        => Ok(await svc.GetAccessoriesAsync(TenantId, ct));

    [HttpPost("accesorios")]
    public async Task<IActionResult> CreateAccessory([FromBody] CreateCatalogItemRequest req, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.CreateAccessoryAsync(TenantId, req.Name, ct));
    }

    [HttpGet("ubicaciones")]
    public async Task<IActionResult> GetLocations(CancellationToken ct = default)
        => Ok(await svc.GetLocationsAsync(TenantId, ct));

    [HttpPost("ubicaciones")]
    public async Task<IActionResult> CreateLocation([FromBody] CreateCatalogItemRequest req, CancellationToken ct = default)
    {
        if (!IsOwner) return Forbid();
        return Ok(await svc.CreateLocationAsync(TenantId, req.Name, ct));
    }
}

public record CreateCatalogModelRequest(string Name, string IdType = "IMEI");
public record CreateCatalogItemRequest(string Name);

// ─── TC BLUE ───────────────────────────────────────────────
[Route("api/tc-blue")]
public class TcBlueController(ITcBlueService svc) : TenantBaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
        => Ok(new { rate = await svc.GetCurrentRateAsync(ct) });
}

// ─── AGENDA ───────────────────────────────────────────────

[Route("api/agenda")]
public class AgendaController(IAgendaService svc) : TenantBaseController
{
  [HttpGet]
  public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month, CancellationToken ct = default)
    => Ok(await svc.GetByMonthAsync(TenantId, year, month, ct));

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateCalendarEventDto dto, CancellationToken ct = default)
    => Ok(await svc.CreateAsync(TenantId, dto, ct));

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
  {
    await svc.DeleteAsync(TenantId, id, ct);
    return NoContent();
  }
}
