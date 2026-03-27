using CleanBrilliant.Data; // Added to access UnitOfWork
using CleanBrilliant.Data.Gateways;
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

// --- Core Data Services ---
builder.Services.AddScoped<UnitOfWork>(); // 1. FIX: Registered UnitOfWork

// --- Database Gateways ---
builder.Services.AddScoped<PreShipmentGateway>();
builder.Services.AddScoped<DashboardLayoutGateway>(); // 2. FIX: Registered DashboardLayoutGateway
builder.Services.AddScoped<ProductDetailGateway>();

// --- Strategies ---
builder.Services.AddScoped<ICarbonFootprintStrategy, ProductCF>();

// --- Services & Controls ---
// Dashboard Layout
builder.Services.AddScoped<DashboardLayoutControl>();
builder.Services.AddScoped<ILayoutWidgetManager>(provider => provider.GetRequiredService<DashboardLayoutControl>());

// Widgets
builder.Services.AddScoped<IWidgetBuilder, WidgetBuilder>();
builder.Services.AddScoped<WidgetControl>();
builder.Services.AddScoped<IWidgetManager>(provider => provider.GetRequiredService<WidgetControl>());

// Carbon & Analytics
builder.Services.AddSingleton<CoefficientControl>();
builder.Services.AddScoped<IAggregatedData, Co2AnalyticsControl>();

// Calculators (Registered concretely, then mapped to interfaces to share instances cleanly)
builder.Services.AddScoped<ProductCalculator>();
builder.Services.AddScoped<IProductDetailWriter>(provider => provider.GetRequiredService<ProductCalculator>());
builder.Services.AddScoped<IProductDetailReader>(provider => provider.GetRequiredService<ProductCalculator>());

builder.Services.AddScoped<PreShipmentCarbonCalculator>();
builder.Services.AddScoped<IPreShipmentCarbonReader>(provider => provider.GetRequiredService<PreShipmentCarbonCalculator>());
builder.Services.AddScoped<IPreShipmentCarbonWriter>(provider => provider.GetRequiredService<PreShipmentCarbonCalculator>());

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

await using (var scope = app.Services.CreateAsyncScope())
{
    var dashboardLayoutControl = scope.ServiceProvider.GetRequiredService<DashboardLayoutControl>();
    await dashboardLayoutControl.EnsureDefaultLayoutExists();
}

app.Run();