using System;
using System.Threading;
using MatrixServer.Packets;
using Serilog;
using Shared.Udp;

namespace MatrixServer;

internal class MatrixServer : PacketServer
{
    public MatrixServer(MatrixServerSettings matrixServerSettings,
                        ILogger logger)
        : base(matrixServerSettings.Port, logger)
    {
    }

    protected override void HandlePacket(Packet packet, CancellationToken ct)
    {
        var mem = packet.PacketData;
        var socketId = Deserializer.ReadStruct<uint>(mem);
        if (socketId != 0)
        {
            return;
        }

        Logger.Verbose("[MATRIX] " + packet.RemoteEndpoint + " sent " + packet.PacketData.Length + " bytes.");

        var matrixPkt = Deserializer.ReadStruct<MatrixPacketBase>(mem);

        switch (matrixPkt.Type)
        {
            case "POKE": // POKE
                Deserializer.ReadStruct<MatrixPacketPoke>(mem);
                Logger.Verbose("[POKE]");
                var nextSocketId = GenerateSocketId();
                Logger.Information("Assigning SocketID [" + nextSocketId + "] to [" + packet.RemoteEndpoint + "]");
                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHehe(nextSocketId)), packet.RemoteEndpoint);
                break;
            case "KISS": // KISS
                Deserializer.ReadStruct<MatrixPacketKiss>(mem);
                Logger.Verbose("[KISS]");
                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHugg(1, 25001)), packet.RemoteEndpoint);
                break;
            case "ABRT": // ABRT
                Deserializer.ReadStruct<MatrixPacketAbrt>(mem);
                Logger.Verbose("[ABRT]");
                break;
            default:
                Logger.Error("Unknown Matrix Packet Type: " + matrixPkt.Type);
                return;
        }
    }

    private static uint GenerateSocketId()
    {
        return unchecked((uint)((0xff00ff << 8) | new Random().Next(0, 256)));
    }
}