using Microsoft.AspNetCore.HttpLogging;

namespace CrowdsecDotnetDemo;

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

public class CountingStream : Stream
{
    private readonly Stream _inner;
    public long BytesWritten { get; private set; }

    public CountingStream(Stream inner) => _inner = inner;

    public override void Write(byte[] buffer, int offset, int count)
    {
        BytesWritten += count;
        _inner.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        BytesWritten += count;
        await _inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    // Delegate other members to _inner
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }
    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
}