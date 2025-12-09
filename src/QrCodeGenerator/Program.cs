using System.ComponentModel;
using System.Net.Mime;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Net.Http.Headers;
using QRCoder;
using TinyHelpers.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("QrCodeGenerationRateLimiter", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ =>
            new()
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window))
        {
            var response = context.HttpContext.Response;
            response.Headers.RetryAfter = window.TotalSeconds.ToString();
        }

        return ValueTask.CompletedTask;
    };
});

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("QrCodeGenerationCachePolicy", policy =>
    {
        policy.Expire(TimeSpan.FromHours(24));
    });
});

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true)
            .AllowCredentials().WithExposedHeaders(HeaderNames.ContentDisposition);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders(new()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { }
});

app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();

app.UseOutputCache();
app.UseRateLimiter();

app.MapGet("/qrcode", ([AsParameters] QrCodeSettings qrCodeSettings) =>
{
    var qrCodeImage = PngByteQRCodeHelper.GetQRCode(qrCodeSettings.Content, QRCodeGenerator.ECCLevel.Q,
        qrCodeSettings.Size.GetValueOrDefault(3), qrCodeSettings.DrawBorder.GetValueOrDefault(true));

    return TypedResults.File(qrCodeImage, MediaTypeNames.Image.Png);
})
.Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Image.Png)
.CacheOutput("QrCodeGenerationCachePolicy")
.RequireRateLimiting("QrCodeGenerationRateLimiter");

app.Run();

public class QrCodeSettings
{
    public required string Content { get; set; }

    [DefaultValue(3)]
    public int? Size { get; set; }

    [DefaultValue(true)]
    public bool? DrawBorder { get; set; }
}