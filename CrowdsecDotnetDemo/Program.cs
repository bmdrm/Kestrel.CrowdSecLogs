using System.Globalization;
using CrowdsecDotnetDemo;
using Microsoft.AspNetCore.Http.HttpResults;
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
                            HttpLoggingFields.Duration;
    
    options.CombineLogs = true;

});

var app = builder.Build();

app.UseHttpLogging();
app.UseSerilogRequestLogging();

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
        
        var referer = logEvent.Properties.TryGetValue("Referer", out var refererVal)
            ? refererVal.ToString().Trim('"')
            : "-";

        var duration = logEvent.Properties.TryGetValue("Duration", out var durationVal)
            ? durationVal.ToString().Trim('"')
            : "0";
        
        output.WriteLine($"{remoteAddr} - - [{timestamp}] \"{method} {path} {protocol}\" {statusCode} {bodyBytesSent} \"{referer}\" \"{userAgent}\" {duration}");
    }
}