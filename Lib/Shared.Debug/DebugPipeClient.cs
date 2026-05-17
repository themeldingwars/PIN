#nullable enable
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using DebugPipeProto;
using Google.Protobuf;
using Serilog;

public class DebugPipeClient
{
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private NamedPipeClientStream? _pipe;

    public DebugPipeClient(ILogger logger)
    {
        _logger = logger;
    }

    public event Action<PipeMessage>? OnMessage;

    public bool IsConnected => _pipe?.IsConnected == true;

    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _logger.Debug("DebugPipeClient creating pipe");
            _pipe = new NamedPipeClientStream(
                ".",
                "debugpipe",
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            try
            {
                _logger.Debug("DebugPipeClient Trying to connect");
                await _pipe.ConnectAsync(2000, token);
                _logger.Debug("DebugPipeClient Connection! Handling");
                await HandleConnectionAsync(_pipe, token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("DebugPipeClient OperationCanceledException");
                break;
            }
            catch
            {
                _logger.Debug("DebugPipeClient Exception in RunAsync");
                await Task.Delay(2000, token);
            }
            finally
            {
                CleanupPipe();
            }
        }
    }

    public async Task SendAsync(PipeMessage message)
    {
        var pipe = _pipe;
        if (pipe == null || !pipe.IsConnected)
        {
            return;
        }

        await _writeLock.WaitAsync();
        try
        {
            using var ms = new MemoryStream();
            message.WriteTo(ms);

            var data = ms.ToArray();
            var lengthBytes = BitConverter.GetBytes(data.Length);

            await pipe.WriteAsync(lengthBytes);
            await pipe.WriteAsync(data.AsMemory());
            await pipe.FlushAsync();
        }
        catch
        {
            CleanupPipe();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer, int length, CancellationToken token)
    {
        int offset = 0;

        while (offset < length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, length - offset), token);
            if (read == 0)
            {
                throw new IOException("Disconnected");
            }

            offset += read;
        }
    }

    private async Task HandleConnectionAsync(NamedPipeClientStream pipe, CancellationToken token)
    {
        _logger.Debug("DebugPipeClient HandleConnectionAsync");
        var reader = new StreamReader(pipe);

        var lengthBuffer = new byte[4];
        try
        {
            while (!token.IsCancellationRequested && pipe.IsConnected)
            {
                // Read message length
                await ReadExactAsync(pipe, lengthBuffer, 4, token);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // Read message body
                var buffer = new byte[length];
                await ReadExactAsync(pipe, buffer, length, token);

                var message = PipeMessage.Parser.ParseFrom(buffer);

                HandleMessage(message);
            }
        }
        catch
        {
            _logger.Debug("DebugPipeClient Exception in HandleConnectionAsync");
        }
    }

    private void HandleMessage(PipeMessage message)
    {
        // _logger.Debug($"RECV {message}");
        OnMessage?.Invoke(message);
    }

    private void CleanupPipe()
    {
        _logger.Debug("DebugPipeClient CleanupPipe");
        try
        {
            _pipe?.Dispose();
        }
        catch
        {
            _logger.Debug("DebugPipeClient Exception in CleanupPipe");
        }

        _pipe = null;
    }
}