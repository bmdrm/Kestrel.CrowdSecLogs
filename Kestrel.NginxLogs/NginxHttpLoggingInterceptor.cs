using Microsoft.AspNetCore.HttpLogging;

namespace Kestrel.NginxLogs;

public sealed class NginxHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        var remoteIp = logContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        logContext.AddParameter("RemoteIpAddress", remoteIp);

        if (logContext.HttpContext.Request.Headers.TryGetValue("Referer", out var referer))
        {
            logContext.AddParameter("Referer", referer.ToString());
        }
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        var contentLength = logContext.HttpContext.Response.ContentLength;
        logContext.AddParameter("BodyBytesSent", contentLength ?? 0);
        
        return ValueTask.CompletedTask;
    }
}