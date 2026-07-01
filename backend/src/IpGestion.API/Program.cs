using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IpGestion.Application.Interfaces;
using IpGestion.Application.Common.Exceptions;
using IpGestion.Domain.Interfaces;
using IpGestion.Infrastructure.Persistence;
using IpGestion.Infrastructure.Persistence.Repositories;
using IpGestion.Infrastructure.Services;
using IpGestion.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "iP Gestión API", Version = "v1" });
});

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=ipgestion.db"));

// Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IServiceTechService, ServiceTechService>();
builder.Services.AddScoped<ICuentasCorrientesService, CuentasCorrientesService>();
builder.Services.AddScoped<IRetentionService, RetentionService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ITcBlueService, TcBlueService>();
builder.Services.AddScoped<IAgendaService, AgendaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpClient();

// JWT — reads token from HTTP-only cookie "jwt"
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not configured in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        // Keep claim names verbatim ("role", "email", "userId", "tenantId").
        // Otherwise the handler remaps short names to long schema URIs and
        // User.FindFirstValue("role") returns null.
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        // Reads the JWT from the HTTP-only cookie instead of the Authorization header
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };
    });

// CORS: AllowCredentials() requires SetIsOriginAllowed instead of AllowAnyOrigin
builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
    p.SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var u) && u.Host == "localhost")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // applies EF migrations (creates schema on a fresh DB)
    await SeedData.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware order matters:
// 1. UseRouting — match route metadata (needed by UseCors)
// 2. UseCors — add CORS headers early so they survive error responses
// 3. UseAuthentication — validate JWT from cookie, populate HttpContext.User
// 4. UseAuthorization — enforce [Authorize] attributes
// 5. Exception handler — wraps controller code, CORS headers already present
// 6. MapControllers — route to actions
app.UseRouting();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    try
    {
        await next(ctx);
    }
    catch (Exception ex)
    {
        if (ctx.Response.HasStarted) throw;

        ctx.Response.StatusCode = ex switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            BusinessException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapControllers();
app.Run();
