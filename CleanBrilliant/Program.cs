using CleanBrilliant.Data.Gateways;
using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.Control;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Boundary;
using CleanBrilliant.DataSource.Gateways;
using CleanBrilliant.Interfaces;
using CleanBrilliant.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CleanBrilliant.Data.Interfaces;
using CleanBrilliant.Data.DbCon;
using CleanBrilliant.Models.Interfaces;
using CleanBrilliant.Models.CarbonStrategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// --- Database Gateways ---
builder.Services.AddScoped<PreShipmentGateway>();

// --- Strategies ---
builder.Services.AddScoped<ICarbonFootprintStrategy, ProductCF>();

// --- Services & Controls ---
builder.Services.AddSingleton<CoefficientControl>();
builder.Services.AddSingleton<CleanBrilliant.Services.ICoefficientManager>(sp => sp.GetRequiredService<CoefficientControl>());

builder.Services.AddScoped<IProductDetailReader, ProductCalculator>();

// Register calculator once and forward all required interfaces to the same scoped instance
builder.Services.AddScoped<PreShipmentCarbonCalculator>();
builder.Services.AddScoped<CleanBrilliant.Data.Interfaces.IPreShipmentCarbonReader>(sp => sp.GetRequiredService<PreShipmentCarbonCalculator>());
builder.Services.AddScoped<IPreShipmentCarbonWriter>(sp => sp.GetRequiredService<PreShipmentCarbonCalculator>());
builder.Services.AddScoped<IPreShipmentCarbonReader>(sp => sp.GetRequiredService<PreShipmentCarbonCalculator>());

builder.Services.AddScoped<ProductDetailGateway>();
// Register the Calculator under its Interfaces (Business Logic)
builder.Services.AddScoped<IProductDetailWriter, ProductCalculator>();
builder.Services.AddScoped<IProductDetailReader,ProductCalculator>();

// ── Module 3: Carbon Footprint Calculator ──

// Gateway interfaces → implementations
builder.Services.AddScoped<IOutboundDistributionGateway, OutboundDistributionGateway>();
builder.Services.AddScoped<IInboundLogisticsGateway, InboundLogisticsGateway>();
builder.Services.AddScoped<ISupplierTransportCarbonGateway, SupplierTransportCarbonGateway>();
builder.Services.AddScoped<ICustomerTransportCarbonGateway, CustomerTransportCarbonGateway>();
builder.Services.AddScoped<ISupplierCarbonDataGateway, SupplierCarbonDataGateway>();
builder.Services.AddScoped<ICustomerCarbonDataGateway, CustomerCarbonDataGateway>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepositoryGateway>();
builder.Services.AddScoped<IBuildingRepository, BuildingRepositoryGateway>();
builder.Services.AddScoped<IShippingMethodGateway, ShippingMethodGateway>();
builder.Services.AddScoped<ISupplierLookupGateway, SupplierLookupGateway>();

// Boundary adapters
builder.Services.AddHttpClient<IOSRMService, OSRMApiAdapter>();
builder.Services.AddHttpClient<IPostalService, PostalCodeDatabaseAdapter>();

// Factory
builder.Services.AddScoped<ICarbonEntityFactory, CustomerEntityFactory>();

// Control classes (no dependencies)
builder.Services.AddScoped<TransportHubManager>();
builder.Services.AddScoped<CarbonAnalysis>();
builder.Services.AddScoped<ICarbonAnalysisService>(sp => sp.GetRequiredService<CarbonAnalysis>());

// Route calculation (multi-interface: register concrete, then forward interfaces)
builder.Services.AddScoped<OutboundDistribution>();
builder.Services.AddScoped<ICustomerDistanceService>(sp => sp.GetRequiredService<OutboundDistribution>());
builder.Services.AddScoped<IEstimatedTimeService>(sp => sp.GetRequiredService<OutboundDistribution>());
builder.Services.AddScoped<InboundLogistics>();
builder.Services.AddScoped<IRestockDistanceService>(sp => sp.GetRequiredService<InboundLogistics>());

// Carbon calculation (multi-interface)
builder.Services.AddScoped<TransportCarbonManager>();
builder.Services.AddScoped<IDistanceCarbonService>(sp => sp.GetRequiredService<TransportCarbonManager>());
builder.Services.AddScoped<IGetCarbonData>(sp => sp.GetRequiredService<TransportCarbonManager>());
builder.Services.AddScoped<ISaveShippingMethod>(sp => sp.GetRequiredService<TransportCarbonManager>());
builder.Services.AddScoped<CarbonDataAggregator>();
builder.Services.AddScoped<ITotalCarbonService>(sp => sp.GetRequiredService<CarbonDataAggregator>());

// Corporate carbon
builder.Services.AddScoped<EmployeeCarbonManager>();
builder.Services.AddScoped<BuildingCarbonManager>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
