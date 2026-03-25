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

// --- Database Gateways ---
builder.Services.AddScoped<IDashboardLayoutGateway, DashboardLayoutGateway>();
builder.Services.AddScoped<PreShipmentGateway>();

// --- Strategies ---
builder.Services.AddScoped<ICarbonFootprintStrategy, ProductCF>();

// --- Services & Controls ---
builder.Services.AddScoped<DashboardLayoutSerializer>();
builder.Services.AddScoped<DashboardLayoutControl>();
builder.Services.AddScoped<IWidgetBuilder, WidgetBuilder>();
builder.Services.AddScoped<WidgetControl>();

// Register Calculator for both Interfaces to ensure singleton-per-request behavior
builder.Services.AddScoped<IPreShipmentCarbonReader, PreShipmentCarbonCalculator>();
builder.Services.AddScoped<IPreShipmentCarbonWriter, PreShipmentCarbonCalculator>();

// Register Analytics
builder.Services.AddScoped<IAggregatedData, Co2AnalyticsControl>();

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