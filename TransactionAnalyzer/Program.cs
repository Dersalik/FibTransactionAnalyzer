using Microsoft.AspNetCore.Http.Timeouts;
using Transaction;
using TransactionAnalyzer.Models;
using TransactionAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RequestTimeoutOptions>(options =>
{
    options.AddPolicy("MyTimeoutPolicy", TimeSpan.FromSeconds(5));
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITransactionReader, TransactionReader>();
builder.Services.AddScoped<ITransactionAnalysisService, TransactionAnalysisService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "analysis",
//    pattern: "Analysis/{action=Index}/{id?}",
//    defaults: new { controller = "Analysis" });

app.Run();


public partial class Program { }
