using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Shared.Udp;

public sealed class HealthEndpoint : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Func<bool> _isReady;
    private readonly ILogger _logger;

    public HealthEndpoint(string prefix, Func<bool> isReady, ILogger logger)
    {
        _isReady = isReady;
        _logger = logger;

        _listener.Prefixes.Add(prefix.EndsWith('/') ? prefix : prefix + "/");
    }

    public void Start(CancellationToken ct)
    {
        _listener.Start();
        _logger.Information("Health endpoint listening on {Prefixes}", _listener.Prefixes);
        _ = Task.Run(() => ServeAsync(ct), ct);
    }

    public void Dispose()
    {
        _listener.Close();
    }

    private async Task ServeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (ct.IsCancellationRequested || !_listener.IsListening)
            {
                break;
            }
            catch (HttpListenerException ex)
            {
                _logger.Warning(ex, "Health endpoint failed to accept a request");
                continue;
            }

            var ready = _isReady();
            var body = Encoding.UTF8.GetBytes(ready ? "Healthy" : "Starting");
            context.Response.StatusCode = ready ? 200 : 503;
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = body.Length;
            await context.Response.OutputStream.WriteAsync(body, ct);
            context.Response.Close();
        }
    }
}
