#nullable enable
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using DebugPipeProto;
using Google.Protobuf;
using Serilog;

public class DebugPipeServer
{
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private NamedPipeServerStream? _pipe;
    private volatile bool _connectionBroken;

    public DebugPipeServer(ILogger logger)
    {
        _logger = logger;
    }

    public event Action<PipeMessage>? OnMessage;
    public event Action? OnConnection;

    public bool IsConnected => _pipe?.IsConnected == true;

    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // _logger.Debug("DebugPipeServer creating pipe");
            _pipe = new NamedPipeServerStream(
                "debugpipe",
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                _logger.Debug("DebugPipeServer Ready for connection");
                await _pipe.WaitForConnectionAsync(token);
                _connectionBroken = false;
                _logger.Debug("DebugPipeServer Accepted connection");
                await HandleClientAsync(_pipe, token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("DebugPipeServer OperationCanceledException");
                break;
            }
            catch
            {
                _logger.Debug("DebugPipeServer Exception in RunAsync");
            }
            finally
            {
                // _logger.Debug("DebugPipeServer CleanupPipe from RunAsync");
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
            _logger.Debug("DebugPipeServer SendAsync Sets _connectionBroken");
            _connectionBroken = true;
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

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken token)
    {
        // _logger.Debug("DebugPipeServer HandleClientAsync");
        OnConnection?.Invoke();

        var lengthBuffer = new byte[4];
        try
        {
            while (!token.IsCancellationRequested && pipe.IsConnected && !_connectionBroken)
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
            _logger.Debug("DebugPipeServer Exception in HandleClientAsync");
        }
    }

    private void HandleMessage(PipeMessage message)
    {
        // _logger.Debug($"RECV {message}");
        OnMessage?.Invoke(message);
    }

    private void CleanupPipe()
    {
        // _logger.Debug("DebugPipeServer CleanupPipe");
        try
        {
            _pipe?.Dispose();
        }
        catch
        {
            // _logger.Debug("DebugPipeServer Exception in CleanupPipe");
        }

        _pipe = null;
    }
}