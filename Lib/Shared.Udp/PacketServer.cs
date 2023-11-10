using System;
using System.Net;
using System.Net.Sockets;
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

    protected PacketServer(ushort port, ILogger logger)
    {
        Logger = logger;
        ListenEndpoint = new IPEndPoint(IPAddress.Any, port);
        ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public bool IsRunning { get; protected set; }

    // TODO: Move to separate thread? add console/rcon handling here?
    // FIXME: Move timing to GameServer
    public void Run()
    {
        Source = new CancellationTokenSource();
        var ct = Source.Token;

        IncomingPackets = new BufferBlock<Packet?>();
        OutgoingPackets = new BufferBlock<Packet?>();

        var listenThread = Utils.RunThread(ListenThreadAsync, ct);
        var runThread = Utils.RunThread(ServerRunThreadAsync, ct);
        var sendThread = Utils.RunThread(SendThreadAsync, ct);

        Startup(ct);

        IsRunning = true;

        while (IsRunning)
        {
            // TODO: Handle Command
            var line = Console.ReadLine();
            HandleCommand(line);
        }

        if (!Source.IsCancellationRequested)
        {
            Source.Cancel();
        }

        Shutdown(ct);
    }

    public async Task<bool> SendAsync(Memory<byte> packet, IPEndPoint endPoint)
    {
        return await OutgoingPackets.SendAsync(new Packet(endPoint, packet));
    }

    protected virtual void HandleCommand(string line)
    {
        if (line.Trim().StartsWith("exit"))
        {
            IsRunning = false;
            Source.Cancel();
        }
    }

    protected abstract void HandlePacket(Packet p, CancellationToken ct);
    protected virtual void Startup(CancellationToken ct)
    {
    }

    protected virtual async void ServerRunThreadAsync(CancellationToken ct)
    {
        Packet? p;
        while ((p = await IncomingPackets.ReceiveAsync(ct)) != null)
        {
            HandlePacket(p.Value, ct);
        }
    }

    protected virtual void Shutdown(CancellationToken ct)
    {
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
                    buffer.AsSpan()[..numberOfBytesReceived].ToArray().CopyTo(buf, 0);
                    _ = await IncomingPackets.SendAsync(new Packet((IPEndPoint)remoteEndPoint, new ReadOnlyMemory<byte>(buf, 0, numberOfBytesReceived), DateTime.Now), ct);

                    // Not 100% sure this needs to be cleared?
                    remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error {0}", "listenThread");
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

            Packet? packet;
            while ((packet = await OutgoingPackets.ReceiveAsync(ct)) != null)
            {
                _ = ServerSocket.SendTo(packet.Value.PacketData.ToArray(), packet.Value.PacketData.Length, SocketFlags.None, packet.Value.RemoteEndpoint);
            }

            _ = Thread.Yield();
        }
    }
}