using Serilog;

namespace Microsoft.AspNetCore.Builder;

public static class CrowdSecApplicationBuilderExtensions
{
   public static IApplicationBuilder UseCrowdSecLogsExporter(this IApplicationBuilder app)
   {
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

      return app;
   }
}