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
            .WriteTo.File(new NginxLogFormatter() ,"Logs/http-.log");
    })
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSingleton<IHttpLoggingInterceptor, NginxHttpLoggingInterceptor>();


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