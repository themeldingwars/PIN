using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace GameServer.Logging;

public class EntityIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var targetKeys = logEvent.Properties
            .Where(p => p.Key is "Entity" or "EntityId")
            .ToList();

        foreach (var prop in targetKeys)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(prop.Key, new ScalarValue(prop.Value.ToString())));
        }
    }
}
