using Microsoft.AspNetCore.HttpLogging;

namespace CrowdsecDotnetDemo;

internal sealed class CustomHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        var request = logContext.HttpContext.Request;
    
        var remoteIp = logContext.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
        logContext.AddParameter("RemoteIpAddress", remoteIp);
    
        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
    
        var contentLength = logContext.HttpContext.Response.ContentLength;
        logContext.AddParameter("bodyBytesSent", contentLength ?? 0);

        return ValueTask.CompletedTask;
    }
}