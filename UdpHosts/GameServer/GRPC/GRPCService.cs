using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GameServer.GRPC.EventHandlers;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcGameServerAPIClient;
using Serilog;

namespace GameServer.GRPC;

public static class GRPCService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(GRPCService));

    // Shard threads and the loot/admin systems all send commands, and the request stream only takes one writer at a time
    private static readonly SemaphoreSlim _writeLock = new(1, 1);

    private static GrpcChannel _channel;
    private static GameServerAPI.GameServerAPIClient _client;
    private static AsyncDuplexStreamingCall<Command, Event> _stream;

    public static void Init(string address)
    {
        _channel = GrpcChannel.ForAddress(address);
        _client = new GameServerAPI.GameServerAPIClient(_channel);
    }

    public static bool IsAvailable()
    {
        return _channel.State == ConnectivityState.Ready;
    }

    public static async Task<LoginAuthorizationResponse> AuthorizeLoginAsync(byte[] ticket, ulong characterGuid)
    {
        return await _client.AuthorizeLoginAsync(new LoginAuthorizationRequest
        {
            Ticket = ByteString.CopyFrom(ticket), CharacterGuid = characterGuid
        });
    }

    public static async Task<BattleframeProgressionResponse> GetBattleframeProgressionAsync(long characterId)
    {
        return await _client.GetBattleframeProgressionAsync(new CharacterID { ID = characterId });
    }

    public static async Task<CharacterAndBattleframeVisuals> GetCharacterAndBattleframeVisualsAsync(long characterId)
    {
        return await _client.GetCharacterAndBattleframeVisualsAsync(new CharacterID { ID = characterId });
    }

    public static async Task<CharacterInventoryResponse> GetCharacterInventoryAsync(long characterId)
    {
        return await _client.GetCharacterInventoryAsync(new CharacterID { ID = characterId });
    }

    public static async Task<CharacterUnlocksResponse> GetCharacterUnlocksAsync(long characterId)
    {
        return await _client.GetCharacterUnlocksAsync(new CharacterID { ID = characterId });
    }

    public static async Task SaveCharacterSessionDataAsync(ulong characterId, uint zoneId, uint outpostId, uint timePlayed)
    {
        var data = new SaveGameSessionData()
           {
               CharacterId = characterId, ZoneId = zoneId, OutpostId = outpostId, TimePlayed = timePlayed
           };

        await SendCommandAsync(new Command() { SaveGameSessionData = data });
    }

    /// <summary>
    ///     Saves the zone a character is moving to and returns once RIN has written it, unlike the session save over the stream
    /// </summary>
    /// <param name="characterId">The full character guid, with the character type in its last byte</param>
    /// <param name="zoneId">The zone the character is moving to</param>
    /// <param name="outpostId">The outpost the character spawns at in the new zone</param>
    /// <param name="timePlayed">Seconds the character spent in the zone it is leaving</param>
    /// <returns>True when RIN has written the new zone; false when it refused the transfer. A failed rpc throws instead of returning false</returns>
    public static async Task<bool> TransferCharacterAsync(ulong characterId, uint zoneId, uint outpostId, uint timePlayed)
    {
        var resp = await _client.TransferCharacterAsync(new TransferCharacterReq
        {
            CharacterId = characterId, ZoneId = zoneId, OutpostId = outpostId, TimePlayed = timePlayed
        });

        return resp.Success;
    }

    /// <summary>
    ///     Sends a command over the stream. Commands are fire and forget: RIN never acks them and never reports a
    ///     validation or database error back, so a lost command is only visible here in the log.
    /// </summary>
    /// <param name="command">The command to send</param>
    /// <returns>An awaitable task. It completes normally even when the command is dropped or the write fails, since both only go to the log</returns>
    public static async Task SendCommandAsync(Command command)
    {
        var stream = _stream;
        if (stream == null)
        {
            _logger.Warning("Dropping GRPC command {Command}, no stream is open", command.SubtypeCase);
            return;
        }

        await _writeLock.WaitAsync();
        try
        {
            await stream.RequestStream.WriteAsync(command);
        }
        catch (Exception e)
        {
            // A database error on the RIN side tears the whole stream down. ListenAsync reconnects, so only this
            // command is lost and the next one goes over the new stream.
            _logger.Warning(e, "Failed to send GRPC command {Command}", command.SubtypeCase);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public static async Task ListenAsync(ConcurrentDictionary<uint, INetworkPlayer> clientMap, CancellationToken ct)
    {
        _stream = _client.Stream(cancellationToken: ct);

        try
        {
            await foreach (var evt in _stream.ResponseStream.ReadAllAsync(ct))
            {
                _logger.Information("{Event}", evt);
                switch (evt.SubtypeCase)
                {
                    case Event.SubtypeOneofCase.ArmyApplicationApproved:
                        ArmyEventHandler.HandleEvent(evt.ArmyApplicationApproved, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyApplicationReceived:
                        ArmyEventHandler.HandleEvent(evt.ArmyApplicationReceived, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyApplicationRejected:
                        ArmyEventHandler.HandleEvent(evt.ArmyApplicationRejected, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyApplicationsUpdated:
                        ArmyEventHandler.HandleEvent(evt.ArmyApplicationsUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyIdChanged:
                        ArmyEventHandler.HandleEvent(evt.ArmyIdChanged, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyInfoUpdated:
                        ArmyEventHandler.HandleEvent(evt.ArmyInfoUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyInviteApproved:
                        ArmyEventHandler.HandleEvent(evt.ArmyInviteApproved, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyInviteReceived:
                        ArmyEventHandler.HandleEvent(evt.ArmyInviteReceived, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyInviteRejected:
                        ArmyEventHandler.HandleEvent(evt.ArmyInviteRejected, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyMembersUpdated:
                        ArmyEventHandler.HandleEvent(evt.ArmyMembersUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyRanksUpdated:
                        ArmyEventHandler.HandleEvent(evt.ArmyRanksUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.ArmyTagUpdated:
                        ArmyEventHandler.HandleEvent(evt.ArmyTagUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.CharacterVisualsUpdated:
                        CharacterEventHandler.HandleEvent(evt.CharacterVisualsUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.InventoryUpdated:
                        InventoryEventHandler.HandleEvent(evt.InventoryUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.UnlocksUpdated:
                        UnlockEventHandler.HandleEvent(evt.UnlocksUpdated, clientMap);
                        break;
                    case Event.SubtypeOneofCase.None:
                    default:
                        break;
                }
            }
        }
        finally
        {
            // Stop sending into a stream that is gone; ListenAsync opens a new one on the next pass
            _stream = null;
        }
    }
}
