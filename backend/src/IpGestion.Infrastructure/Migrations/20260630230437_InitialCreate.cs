using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpGestion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", nullable: false),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    Condition = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmployeeBox = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmployeeUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashClosings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCierre = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IngresosHoy = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    EgresosHoy = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    LiquidezFinalUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashClosings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogAccessories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RequiresModel = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresColor = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogAccessories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IdType = table.Column<string>(type: "TEXT", nullable: false),
                    RequiresStorage = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresColor = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiresSize = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompetitorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", nullable: false),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: true),
                    PriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Competitors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovementCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObjectionResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RawText = table.Column<string>(type: "TEXT", nullable: false),
                    ClusterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SuggestedResponse = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectionResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RetentionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RuleType = table.Column<string>(type: "TEXT", nullable: false),
                    DaysAfterSale = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetentionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    LogoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Plan = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceAlertDays = table.Column<int>(type: "INTEGER", nullable: false),
                    WarrantyPolicyText = table.Column<string>(type: "TEXT", nullable: true),
                    UsedWarrantyDays = table.Column<int>(type: "INTEGER", nullable: false),
                    CloserCommissionEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CloserCommissionType = table.Column<int>(type: "INTEGER", nullable: false),
                    CloserCommissionValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    WholesalePriceEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArsTrCommissionEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArsTrCommissionPercentage = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeInValuations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", nullable: false),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: false),
                    BatteryRange = table.Column<string>(type: "TEXT", nullable: false),
                    BaseValueUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeInValuations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockBulks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    CostUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    SuggestedPriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBulks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBulks_CatalogAccessories_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "CatalogAccessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockBulks_CatalogLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CatalogLocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CashMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CajaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    ExchangeRateUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    MovementCategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReferenceType = table.Column<string>(type: "TEXT", nullable: true),
                    Detail = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashMovements_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashMovements_MovementCategories_MovementCategoryId",
                        column: x => x.MovementCategoryId,
                        principalTable: "MovementCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PriceListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PriceListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemKind = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceListItems_PriceLists_PriceListId",
                        column: x => x.PriceListId,
                        principalTable: "PriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Instagram = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentNumber = table.Column<string>(type: "TEXT", nullable: true),
                    AddressCity = table.Column<string>(type: "TEXT", nullable: true),
                    AddressStreet = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingBranch = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingCity = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingProvince = table.Column<string>(type: "TEXT", nullable: true),
                    ShippingNotes = table.Column<string>(type: "TEXT", nullable: true),
                    PreferredTransport = table.Column<string>(type: "TEXT", nullable: true),
                    PriceListId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantInvitations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    InvitedEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowedModules = table.Column<string>(type: "TEXT", nullable: false),
                    IsCloser = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityBalances",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BalanceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityBalances", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_EntityBalances_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CourierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    TotalUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportOrders_Entities_CourierId",
                        column: x => x.CourierId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportOrders_Entities_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Entities_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Purchases_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RetailClientName = table.Column<string>(type: "TEXT", nullable: true),
                    RetailClientPhone = table.Column<string>(type: "TEXT", nullable: true),
                    RetailClientInstagram = table.Column<string>(type: "TEXT", nullable: true),
                    SaleCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    Origin = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    WarrantyDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CloserIds = table.Column<string>(type: "TEXT", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sales_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceClientJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SvCode = table.Column<string>(type: "TEXT", nullable: false),
                    RetailClientName = table.Column<string>(type: "TEXT", nullable: false),
                    RetailClientPhone = table.Column<string>(type: "TEXT", nullable: true),
                    DeviceModel = table.Column<string>(type: "TEXT", nullable: true),
                    ImeiSerial = table.Column<string>(type: "TEXT", nullable: true),
                    IssueDescription = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PriceToClientUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    TechnicianCostUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    DepositMethod = table.Column<int>(type: "INTEGER", nullable: true),
                    DepositAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceClientJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceClientJobs_Entities_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AccessoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Condition = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    CostProvUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    WeightGrams = table.Column<int>(type: "INTEGER", nullable: false),
                    CourierUsdPerKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    FleteUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CostoFinalUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DetailObs = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportOrderItems_ImportOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ImportOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InternalCode = table.Column<string>(type: "TEXT", nullable: true),
                    ImeiSerial = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    StorageGb = table.Column<int>(type: "INTEGER", nullable: true),
                    SizeMm = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    Condition = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionGrade = table.Column<string>(type: "TEXT", nullable: true),
                    BatteryPct = table.Column<int>(type: "INTEGER", nullable: true),
                    CostUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    SuggestedPriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    WholesalePriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    AccountEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PurchaseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TradeInSaleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockItems_CatalogLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CatalogLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockItems_CatalogModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "CatalogModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockItems_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockItems_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleCloserCommissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CloserUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    PeriodMonth = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleCloserCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleCloserCommissions_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RetailClientName = table.Column<string>(type: "TEXT", nullable: true),
                    RetailClientPhone = table.Column<string>(type: "TEXT", nullable: true),
                    RetailClientInstagram = table.Column<string>(type: "TEXT", nullable: true),
                    StockItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StockBulkId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SaleCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    PickupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DepositAmountUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    AgreedPriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_StockBulks_StockBulkId",
                        column: x => x.StockBulkId,
                        principalTable: "StockBulks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StockItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StockBulkId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItems_StockBulks_StockBulkId",
                        column: x => x.StockBulkId,
                        principalTable: "StockBulks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleItems_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceStockJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StockItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IssueDescription = table.Column<string>(type: "TEXT", nullable: false),
                    AgreedCostUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LimitDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceStockJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceStockJobs_Entities_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Entities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceStockJobs_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenceType = table.Column<string>(type: "TEXT", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    ExchangeRateUsd = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    PurchaseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ReservationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SaleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionPayments_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_CajaId",
                table: "CashMovements",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_CashMovements_MovementCategoryId",
                table: "CashMovements",
                column: "MovementCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_TenantId_Type",
                table: "Entities",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportOrderItems_OrderId",
                table: "ImportOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOrders_CourierId",
                table: "ImportOrders",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportOrders_ProviderId",
                table: "ImportOrders",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceListItems_PriceListId",
                table: "PriceListItems",
                column: "PriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ProviderId",
                table: "Purchases",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_TenantId",
                table: "Purchases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EntityId",
                table: "Reservations",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StockBulkId",
                table: "Reservations",
                column: "StockBulkId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_StockItemId",
                table: "Reservations",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TenantId",
                table: "Reservations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleCloserCommissions_SaleId",
                table: "SaleCloserCommissions",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_StockBulkId",
                table: "SaleItems",
                column: "StockBulkId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_StockItemId",
                table: "SaleItems",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_EntityId",
                table: "Sales",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_TenantId_SaleDate",
                table: "Sales",
                columns: new[] { "TenantId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClientJobs_TechnicianId",
                table: "ServiceClientJobs",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClientJobs_TenantId_Status",
                table: "ServiceClientJobs",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStockJobs_StockItemId",
                table: "ServiceStockJobs",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStockJobs_TechnicianId",
                table: "ServiceStockJobs",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBulks_AccessoryId",
                table: "StockBulks",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBulks_LocationId",
                table: "StockBulks",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_LocationId",
                table: "StockItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ModelId",
                table: "StockItems",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_PurchaseId",
                table: "StockItems",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_TenantId_Status",
                table: "StockItems",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_TenantId_Status",
                table: "TenantInvitations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_Token",
                table: "TenantInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_Email",
                table: "TenantUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUsers_TenantId",
                table: "TenantUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_PurchaseId",
                table: "TransactionPayments",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_ReservationId",
                table: "TransactionPayments",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPayments_SaleId",
                table: "TransactionPayments",
                column: "SaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodeMappings");

            migrationBuilder.DropTable(
                name: "CalendarEvents");

            migrationBuilder.DropTable(
                name: "CashClosings");

            migrationBuilder.DropTable(
                name: "CashMovements");

            migrationBuilder.DropTable(
                name: "CompetitorPrices");

            migrationBuilder.DropTable(
                name: "Competitors");

            migrationBuilder.DropTable(
                name: "EntityBalances");

            migrationBuilder.DropTable(
                name: "ImportOrderItems");

            migrationBuilder.DropTable(
                name: "ObjectionResponses");

            migrationBuilder.DropTable(
                name: "PriceListItems");

            migrationBuilder.DropTable(
                name: "RetentionRules");

            migrationBuilder.DropTable(
                name: "SaleCloserCommissions");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "ServiceClientJobs");

            migrationBuilder.DropTable(
                name: "ServiceStockJobs");

            migrationBuilder.DropTable(
                name: "TenantInvitations");

            migrationBuilder.DropTable(
                name: "TenantUsers");

            migrationBuilder.DropTable(
                name: "TradeInValuations");

            migrationBuilder.DropTable(
                name: "TransactionPayments");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "MovementCategories");

            migrationBuilder.DropTable(
                name: "ImportOrders");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "StockBulks");

            migrationBuilder.DropTable(
                name: "StockItems");

            migrationBuilder.DropTable(
                name: "CatalogAccessories");

            migrationBuilder.DropTable(
                name: "CatalogLocations");

            migrationBuilder.DropTable(
                name: "CatalogModels");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
