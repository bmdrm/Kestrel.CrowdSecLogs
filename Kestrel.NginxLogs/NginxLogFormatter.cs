using System.Globalization;
using System.Net;
using Serilog.Events;
using Serilog.Formatting;

namespace Kestrel.NginxLogs;

internal sealed class NginxLogFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var localTime = logEvent.Timestamp.ToLocalTime();
        var timestamp = $"{localTime.ToString("dd/MMM/yyyy:HH:mm:ss",  CultureInfo.InvariantCulture)} {localTime.ToString("zz")}{localTime.Offset:mm}";
        
        var remoteAddr = logEvent.Properties.TryGetValue("RemoteAddr", out var remoteAddrVal)
            ? remoteAddrVal.ToString().Trim('"')
            : "-";

        var method = logEvent.Properties.TryGetValue("RequestMethod", out var methodVal)
            ? methodVal.ToString().Trim('"')
            : "-";

        var path = logEvent.Properties.TryGetValue("RequestPath", out var pathVal)
            ? pathVal.ToString().Trim('"')
            : "-";

        var statusCode = logEvent.Properties.TryGetValue("StatusCode", out var statusVal)
            ? statusVal.ToString().Trim('"')
            : "-";

        var userAgent = logEvent.Properties.TryGetValue("User-Agent", out var uaVal)
            ? uaVal.ToString().Trim('"')
            : "-";
        var protocol = logEvent.Properties.TryGetValue("Protocol", out var protocolVal)
            ? protocolVal.ToString().Trim('"')
            : "-";

        //TODO: make sure this is the correct value for body bytes sent.
        var bodyBytesSent = logEvent.Properties.TryGetValue("Request-Body-Size", out var bodyBytesSentVal)
            ? bodyBytesSentVal.ToString().Trim('"')
            : "-";
        
        var referer = logEvent.Properties.TryGetValue("Referer", out var refererVal)
            ? refererVal.ToString().Trim('"')
            : "-";

        var duration = logEvent.Properties.TryGetValue("Elapsed", out var durationVal)
            ? durationVal.ToString().Trim('"')
            : "0";
        
        output.WriteLine($"{remoteAddr} - - [{timestamp}] \"{method} {path} {protocol}\" {statusCode} {bodyBytesSent} \"{referer}\" \"{userAgent}\" {duration}");
    }
}