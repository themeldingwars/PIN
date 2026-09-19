using System;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Shared.Common;

public static class OpenTelemetryLogging
{
    public static LoggerConfiguration WriteToOpenTelemetryIfEnabled(this LoggerConfiguration logger)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")))
        {
            return logger;
        }

        return logger.WriteTo.OpenTelemetry(options =>
        {
            // The sink doesn't pick up the service name by itself
            var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
            if (!string.IsNullOrEmpty(serviceName))
            {
                options.ResourceAttributes["service.name"] = serviceName;
            }

            options.IncludedData |= IncludedData.TraceIdField | IncludedData.SpanIdField;
        });
    }
}
