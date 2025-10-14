using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using Kestrel.NginxLogs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Logger(lc =>
    {
        lc.Filter.ByIncludingOnly(Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware"))
            .WriteTo.File(new NginxLogFormatter(), "Logs/http-.log");
    })
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton<IHttpLoggingInterceptor, NginxHttpLoggingInterceptor>();


/*builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                            HttpLoggingFields.RequestQuery |
                            HttpLoggingFields.ResponsePropertiesAndHeaders |
                            HttpLoggingFields.Duration |
                            HttpLoggingFields.All;
    
    options.CombineLogs = true;

});*/

var app = builder.Build();
//app.UseHttpLogging();
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