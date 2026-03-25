using CleanBrilliant.Data.Gateways;
using CleanBrilliant.Interfaces;
using CleanBrilliant.Services;
using Microsoft.EntityFrameworkCore;
using CleanBrilliantProject.Data.DbCon;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Dashboard services
builder.Services.AddScoped<IDashboardLayoutGateway, DashboardLayoutGateway>();
builder.Services.AddScoped<DashboardLayoutSerializer>();
builder.Services.AddScoped<DashboardLayoutControl>();
builder.Services.AddScoped<IWidgetBuilder, WidgetBuilder>();
builder.Services.AddScoped<IAggregatedData, Co2AnalyticsControl>();
builder.Services.AddScoped<WidgetControl>();

builder.Services.AddScoped<ProductDetailGateway>();
// Register the Calculator under its Interfaces (Business Logic)
builder.Services.AddScoped<IProductDetailWriter, ProductCalculator>();
builder.Services.AddScoped<IProductDetailReader,ProductCalculator>();

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
