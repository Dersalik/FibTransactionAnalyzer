using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Transaction;
using TransactionAnalyzer.Models;
using TransactionAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHealthChecks();

builder.Services.AddRequestTimeouts();

builder.Services.Configure<RequestTimeoutOptions>(options =>
{
    options.AddPolicy("MyTimeoutPolicy", TimeSpan.FromSeconds(3));
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("UploadPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(2)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "text/plain";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsync(
            "Upload limit exceeded. Please wait before uploading another file.",
            cancellationToken: token);
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITransactionReader, TransactionReader>();
builder.Services.AddScoped<ITransactionAnalysisService, TransactionAnalysisService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseStaticFiles();

app.UseRouting();

app.UseRequestTimeouts();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


public partial class Program { }
