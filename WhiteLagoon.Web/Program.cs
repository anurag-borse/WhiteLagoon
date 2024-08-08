using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Syncfusion.Licensing;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Implementation;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDbInitalizer, DbInitializer>();
builder.Services.AddScoped<IVillaService, VillaService>();
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

//stripe
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

//sync fusion
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration.GetSection("Syncfusion:LicenseKey").Get<string>());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

SeedDataBase();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDataBase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitalizer>();
        dbInitializer.Initializer();
    }
}