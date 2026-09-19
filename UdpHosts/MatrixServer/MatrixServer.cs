using System;
using System.Threading;
using Aero.Protocol;
using MatrixServer.Packets;
using Serilog;
using Shared.Udp;

namespace MatrixServer;

internal class MatrixServer : PacketServer
{
    private readonly ushort _gameServerPort;

    public MatrixServer(MatrixServerSettings matrixServerSettings,
                        ILogger logger)
        : base(matrixServerSettings.Port, logger)
    {
        _gameServerPort = matrixServerSettings.GameServerPort;
        HealthUrl = matrixServerSettings.HealthUrl;
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
            case "POKE":
                var nextSocketId = GenerateSocketId();
                Logger.Information("Assigning SocketID [{SocketID}] to [{RemoteEndpoint}]", nextSocketId, packet.RemoteEndpoint);

                var poke = Deserializer.ReadStruct<MatrixPacketPoke>(mem);
                var knownProtocol = ProtocolVersions.TryGetMatrixVersion(poke.ProtocolVersion, out var matrixVersion);
                if (!knownProtocol)
                {
                    Logger.Warning("SocketID [{SocketID}] Unknown ProtocolVersion: {ProtocolVersion}", poke.SocketID, poke.ProtocolVersion);
                }
                else
                {
                    Logger.Information("SocketID [{SocketID}] Matrix Protocol {MatrixVersion} ({ProtocolVersion})", nextSocketId, matrixVersion, poke.ProtocolVersion);
                }

                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHehe(nextSocketId)), packet.RemoteEndpoint);
                break;
            case "KISS":
                var kiss = Deserializer.ReadStruct<MatrixPacketKiss>(mem);
                var knownStreamingProtocol = ProtocolVersions.TryGetGssVersion(kiss.StreamingProtocolVersion, out var gssVersion);
                if (!knownStreamingProtocol)
                {
                    Logger.Warning("SocketID [{SocketID}] Unknown StreamingProtocolVersion {StreamingProtocolVersion}", kiss.ReceivedSocketID, kiss.StreamingProtocolVersion);
                }
                else
                {
                    Logger.Information("SocketID [{SocketID}] GSS Protocol {GssVersion} ({StreamingProtocolVersion})", kiss.ReceivedSocketID, gssVersion, kiss.StreamingProtocolVersion);
                }

                _ = SendAsync(Serializer.WriteStruct(new MatrixPacketHugg(1, _gameServerPort)), packet.RemoteEndpoint);
                break;
            case "ABRT":
                var abrt = Deserializer.ReadStruct<MatrixPacketAbrt>(mem);
                Logger.Information("Received abort with reason: {AbortCode}", abrt.Code);
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