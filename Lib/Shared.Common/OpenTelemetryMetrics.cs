using System;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Shared.Common;

public static class OpenTelemetryMetrics
{
    /// <summary>
    ///     Exports the given meters over OTLP when OTEL_EXPORTER_OTLP_ENDPOINT is set, e.g. by Aspire. Returns null otherwise.
    ///     Endpoint, protocol, headers, service name and export interval all come from the standard OTEL_* variables.
    /// </summary>
    public static IDisposable StartIfEnabled(params string[] meters)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")))
        {
            return null;
        }

        return Sdk.CreateMeterProviderBuilder()
                  .AddMeter(meters)
                  .AddOtlpExporter()
                  .Build();
    }
}
