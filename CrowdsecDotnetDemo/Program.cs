using System.Globalization;
using CrowdsecDotnetDemo;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Logger(lc =>
    {
        lc.Filter.ByIncludingOnly(Matching.WithProperty("Duration"))
            .WriteTo.File(new BmDrmFormatter() ,"Logs/http-.log");
    })
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton<IHttpLoggingInterceptor, CustomHttpLoggingInterceptor>();


builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |                                        
                            HttpLoggingFields.RequestQuery |
                            HttpLoggingFields.ResponsePropertiesAndHeaders |
                            HttpLoggingFields.Duration |
                            HttpLoggingFields.All;
    
    options.CombineLogs = true;
});

var app = builder.Build();

app.UseHttpLogging();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
        diagnosticContext.Set("ElapsedMilliseconds", httpContext.Items["ElapsedMilliseconds"] ?? 0);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

app.MapGet("/", async () =>
{
   await Task.Delay(TimeSpan.FromSeconds(1)); 
   return "Hello World";
});
app.Run();


public class BmDrmFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        //var timestamp = logEvent.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var localTime = logEvent.Timestamp.ToLocalTime();
        var timestamp = $"{localTime.ToString("dd/MMM/yyyy:HH:mm:ss",  CultureInfo.InvariantCulture)} {localTime.ToString("zz")}{localTime.Offset:mm}";
        //var timestamp = logEvent.Timestamp.ToLocalTime().ToString("dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture);
        var remoteAddr = logEvent.Properties.TryGetValue("RemoteIpAddress", out var remoteAddrVal)
            ? remoteAddrVal.ToString().Trim('"')
            : "-";

        var method = logEvent.Properties.TryGetValue("Method", out var methodVal)
            ? methodVal.ToString().Trim('"')
            : "-";

        var path = logEvent.Properties.TryGetValue("Path", out var pathVal)
            ? pathVal.ToString().Trim('"')
            : "-";

        var statusCode = logEvent.Properties.TryGetValue("StatusCode", out var statusVal)
            ? statusVal.ToString().Trim('"')
            : "-";

        var elapsed = logEvent.Properties.TryGetValue("Duration", out var elapsedVal)
            ? elapsedVal.ToString().Trim('"')
            : "-";

        var userAgent = logEvent.Properties.TryGetValue("User-Agent", out var uaVal)
            ? uaVal.ToString().Trim('"')
            : "-";
        var protocol = logEvent.Properties.TryGetValue("Protocol", out var protocolVal)
            ? protocolVal.ToString().Trim('"')
            : "-";

        //TODO: make sure this is the correct value for body bytes sent.
        var bodyBytesSent = logEvent.Properties.TryGetValue("BodyBytesSent", out var bodyBytesSentVal)
            ? bodyBytesSentVal.ToString().Trim('"')
            : "-";
        
        output.WriteLine($"{remoteAddr} - - [{timestamp}] \"{method} {path} {protocol}\" {statusCode} {bodyBytesSent} \"{userAgent}\" - ");
    }
}