using CleanBrilliant.Data.Gateways;
using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.Control;
using CleanBrilliant.Services;
using Microsoft.EntityFrameworkCore;
using CleanBrilliantProject.Data.DbCon;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<CategoryManager>();
builder.Services.AddScoped<ProductManager>();

builder.Services.AddScoped<ICategoryGateway, CategoryGateway>();
builder.Services.AddScoped<IProductGateway, ProductGateway>();

// Dashboard services
builder.Services.AddScoped<DashboardLayoutGateway>();
builder.Services.AddScoped<DashboardLayoutSerializer>();
builder.Services.AddScoped<DashboardLayoutControl>();
builder.Services.AddScoped<IWidgetBuilder, WidgetBuilder>();
builder.Services.AddScoped<IAggregatedData, Co2AnalyticsControl>();
builder.Services.AddScoped<WidgetControl>();

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
