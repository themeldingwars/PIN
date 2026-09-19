using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Serilog;

namespace Shared.Udp;

public abstract class PacketServer : IPacketSender
{
    public const int MTU = 1400;

    protected readonly ILogger Logger;

    protected readonly Socket ServerSocket;
    protected readonly IPEndPoint ListenEndpoint;
    protected BufferBlock<Packet?> IncomingPackets;
    protected BufferBlock<Packet?> OutgoingPackets;
    protected CancellationTokenSource Source;

    private readonly ManualResetEventSlim _stopped = new(false);
    private PosixSignalRegistration _sigterm;

    protected PacketServer(ushort port, ILogger logger)
    {
        Logger = logger.ForContext<PacketServer>();
        ListenEndpoint = new IPEndPoint(IPAddress.Any, port);
        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public bool IsRunning { get; private set; }

    protected string HealthUrl { get; init; }

    protected virtual bool IsReady => IsRunning;

    public void Run()
    {
        Source = new CancellationTokenSource();
        var ct = Source.Token;

        using var health = string.IsNullOrEmpty(HealthUrl) ? null : new HealthEndpoint(HealthUrl, () => IsReady, Logger);
        health?.Start(ct);

        IncomingPackets = new BufferBlock<Packet?>();
        OutgoingPackets = new BufferBlock<Packet?>();

        Console.CancelKeyPress += OnCancelKeyPress;

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            _sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
            {
                context.Cancel = true;
                RequestStop();
            });
        }

        var listenThread = Utils.RunThread(ListenThreadAsync, ct);
        var runThread = Utils.RunThread(ServerRunThreadAsync, ct);
        var sendThread = Utils.RunThread(SendThreadAsync, ct);

        Startup(ct);

        IsRunning = true;

        _stopped.Wait();

        IsRunning = false;

        if (!Source.IsCancellationRequested)
        {
            Source.Cancel();
        }

        ServerSocket.Close();
        _sigterm?.Dispose();

        Shutdown(ct);
    }

    /// <summary>
    ///     Requests the server to stop. <see cref="Run" /> returns once the shutdown has been triggered.
    /// </summary>
    public void RequestStop()
    {
        _stopped.Set();
    }

    public async Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint)
    {
        return await OutgoingPackets.SendAsync(new Packet(endPoint, packet));
    }

    protected abstract void HandlePacket(Packet p, CancellationToken ct);
    protected virtual void Startup(CancellationToken ct)
    {
    }

    protected virtual async void ServerRunThreadAsync(CancellationToken ct)
    {
        try
        {
            Packet? p;
            while ((p = await IncomingPackets.ReceiveAsync(ct)) != null)
            {
                HandlePacket(p.Value, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown
        }
    }

    protected virtual void Shutdown(CancellationToken ct)
    {
    }

    private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        RequestStop();
    }

    private async void ListenThreadAsync(CancellationToken ct)
    {
        ServerSocket.Blocking = true;
        ServerSocket.DontFragment = true;
        ServerSocket.ReceiveBufferSize = MTU * 100;
        ServerSocket.SendBufferSize = MTU * 100;
        ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        ServerSocket.Bind(ListenEndpoint);

        Logger.Information("Listening on {0}", ListenEndpoint);

        var buffer = new byte[MTU * 10];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                // Sockets don't support async yet :( Blocking here bc the win api will yield and wait better on the native side than we can here
                int numberOfBytesReceived;
                if ((numberOfBytesReceived = ServerSocket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEndPoint)) > 0)
                {
                    // Should probably change to ArrayPool<byte>, but can't return a Memory<byte> :(
                    // TODO: Move Endpoint and Memory<byte> management to Packet (constructor + destructor)
                    var buf = new byte[numberOfBytesReceived];
                    buffer.AsSpan()[..numberOfBytesReceived].CopyTo(buf);
                    _ = await IncomingPackets.SendAsync(new Packet((IPEndPoint)remoteEndPoint, buf.AsMemory(0, numberOfBytesReceived), DateTime.Now), ct);

                    // Not 100% sure this needs to be cleared?
                    remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!ct.IsCancellationRequested)
                {
                    Logger.Error(ex, "Error {0}", "listenThread");
                }
            }

            _ = Thread.Yield();
        }
    }

    private async void SendThreadAsync(CancellationToken ct)
    {
        while (OutgoingPackets == null)
        {
            Thread.Sleep(10);
        }

        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                Packet? packet;
                while ((packet = await OutgoingPackets.ReceiveAsync(ct)) != null)
                {
                    _ = ServerSocket.SendTo(packet.Value.PacketData.ToArray(), packet.Value.PacketData.Length, SocketFlags.None, packet.Value.RemoteEndpoint);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _ = Thread.Yield();
        }
    }
}