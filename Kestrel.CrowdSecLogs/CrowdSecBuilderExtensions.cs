using Kestrel.CrowdSecLogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Filters;

namespace Microsoft.Extensions.DependencyInjection;

public static class CrowdSecBuilderExtensions
{
   public static IServiceCollection AddCrowdSecLogsExporter(this WebApplicationBuilder builder)
   {
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
                     new CrowdSecLogFormatter(),
                     "/var/tmp/dotnet/access.log",
                     rollOnFileSizeLimit: true,
                     retainedFileCountLimit: 3);
               });
         })
         .CreateLogger();

      builder.Host.UseSerilog();
      builder.Services.AddSingleton<IHttpLoggingInterceptor, CrowdSecHttpLoggingInterceptor>();
      
      return builder.Services;
   }
}