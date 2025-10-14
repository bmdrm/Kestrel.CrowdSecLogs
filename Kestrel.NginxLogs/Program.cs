using Kestrel.NginxLogs;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Logger(lc =>
    {
        lc.Filter.ByIncludingOnly(Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware"))
            .WriteTo.Async(opts =>
            {
                opts.File(
                    new NginxLogFormatter(),
                    "/var/tmp/dotnet/access.log",
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 3);
            });
    })
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton<IHttpLoggingInterceptor, NginxHttpLoggingInterceptor>();

var app = builder.Build();
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
    options.EnrichDiagnosticContext = (diagContext, httpContext) =>
    {
        if (httpContext.Connection.RemoteIpAddress?.MapToIPv4() is { } ipAddress)
        {
            diagContext.Set("RemoteAddr", ipAddress.ToString());
        }
        
        if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
        {
            diagContext.Set("User-Agent", userAgent.ToString());
        }
        
        diagContext.Set("Request-Body-Size", httpContext.Request.ContentLength ?? 0);
        diagContext.Set("Protocol", httpContext.Request.Protocol);
        if (httpContext.Request.Headers.TryGetValue("Referer", out var referer))
        {
            diagContext.Set("Referer", referer.ToString());
        }
    };
});

app.MapGet("/", async () =>
{
   await Task.Delay(TimeSpan.FromSeconds(1));
   return "Hello World";
});


app.Run();
