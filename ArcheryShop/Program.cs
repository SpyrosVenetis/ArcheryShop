using ArcheryShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Currency formatting for Europe culture
var cultureInfo = new CultureInfo("en-IE");
cultureInfo.NumberFormat.CurrencyPositivePattern = 3;
var supportedCultures = new[] { cultureInfo };

var connectionString = builder.Configuration.GetConnectionString("ArcheryShopContext");

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ArcheryShopContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

app.UseRequestLocalization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shop/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();