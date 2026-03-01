using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace GameServer.Logging;

public class LogSystemEnricher : ILogEventEnricher
{
    private static readonly Dictionary<string, string> _overrideMap = new()
    {
        { "Shared.Udp.PacketServer", "Network" },
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue) &&
            sourceContextValue is ScalarValue { Value: string sourceContext })
        {
            var systemName = ExtractSystemName(sourceContext);
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("System", systemName));
        }
        else
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("System", "General"));
        }
    }

    private static string ExtractSystemName(string sourceContext)
    {
        foreach (var (prefix, name) in _overrideMap)
        {
            if (sourceContext.StartsWith(prefix))
            {
                return name;
            }
        }

        var parts = sourceContext.Split('.');
        if (parts.Length >= 2 && parts[0] == "GameServer")
        {
            if (parts.Length >= 3 && parts[1] == "Systems")
            {
                return parts[2];
            }

            return parts[1];
        }

        return "General";
    }
}
