using Transaction;
using TransactionAnalyzer.Models;
using TransactionAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITransactionReader, TransactionReader>();
builder.Services.AddScoped<ITransactionAnalysisService, TransactionAnalysisService>();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "analysis",
//    pattern: "Analysis/{action=Index}/{id?}",
//    defaults: new { controller = "Analysis" });

app.Run();


public partial class Program { }
