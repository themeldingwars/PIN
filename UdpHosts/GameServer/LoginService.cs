using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AeroMessages.Control;
using GameServer.Data;
using GameServer.GRPC;
using GameServer.Protocol;
using GameServer.Systems.CharacterCreation;
using GameServer.Test;
using GrpcGameServerAPIClient;
using Serilog;

namespace GameServer;

/// <summary>
///     Handles the Matrix Login message
/// </summary>
public static class LoginService
{
    /// <summary>
    ///     The expected length of the ticket, anything else is not valid.
    /// </summary>
    private const int _ticketPayloadLength = 35;

    /// <summary>
    ///     This response is a server fault rather than a rejected login.
    /// </summary>
    private const string _rinDatabaseError = "DB_ERROR";

    /// <summary>
    ///     This response indicates the ticket was not in a valid format.
    /// </summary>
    private const string _rinTicketMalformed = "TICKET_MALFORMED";

    private static readonly ILogger _logger = Log.ForContext(typeof(LoginService));

    /// <summary>
    ///     Connections with a login that has been accepted and has not finished yet, keyed by socket id
    /// </summary>
    private static readonly ConcurrentDictionary<uint, byte> _pending = [];

    private static IShardManager _shards;

    public static void Init(IShardManager shards)
    {
        _shards = shards;
    }

    /// <summary>
    ///     Entry point for the Matrix Login message. Runs on the thread of the shard holding the connection:
    ///     the cheap checks run there, the character is fetched off-thread, and the character itself is built
    ///     back on the thread of the shard that ends up owning it.
    /// </summary>
    /// <param name="client">The connection asking to log in</param>
    /// <param name="player">The player the connection belongs to</param>
    /// <param name="characterId">The character id the client asked for, with its type byte in the low bits</param>
    /// <param name="ticket">
    ///     The login ticket the client presented, header included.
    /// </param>
    public static void BeginLogin(NetworkClient client, IPlayer player, ulong characterId, byte[] ticket)
    {
        if (!ValidateConnection(client, player, characterId, out var refusal))
        {
            _logger.Warning("Closing the login of character {CharacterId:X16} on socket {SocketId}: {Refusal}", characterId, client.SocketId, refusal);
            CloseLogin(client);
            return;
        }

        _ = Task.Run(() => LoginAsync(client, characterId, ticket));
    }

    /// <summary>
    ///     The checks that can be made about the connection itself
    /// </summary>
    private static bool ValidateConnection(NetworkClient client, IPlayer player, ulong characterId, out string refusal)
    {
        if (player.CharacterEntity != null)
        {
            refusal = "this connection already has a character";
            return false;
        }

        if (!_pending.TryAdd(client.SocketId, 0))
        {
            refusal = "a login is already in progress";
            return false;
        }

        var guid = characterId & 0xffffffffffffff00;

        if (_shards.Shards.Any(shard => shard.Entities.ContainsKey(guid)))
        {
            _pending.TryRemove(client.SocketId, out _);
            refusal = "the character is already zoned in";
            return false;
        }

        player.CharacterId = guid;
        refusal = null;
        return true;
    }

    /// <summary>
    ///     The rest of the login: Verify whether the login is allowed, retrieve the character, move the
    ///     connection to the right shard and hand the character over to that shard to be created.
    /// </summary>
    private static async Task LoginAsync(NetworkClient client, ulong characterId, byte[] ticket)
    {
        try
        {
            var refusal = await LoginRefusalAsync(ticket, characterId);
            if (refusal != null)
            {
                _logger.Warning("Closing the login of character {CharacterId:X16} on socket {SocketId}: {Refusal}", characterId, client.SocketId, refusal);
                CloseLogin(client);
                return;
            }

            var data = await RetrieveCharacterAsync(characterId);

            // With the hardcoded fallback turned off the character has to arrive whole: a failed fetch,
            // no character row, or an inventory with nothing to equip into ends the login here.
            if (!client.AssignedShard.Settings.AllowHardcodedCharacterData && !data.IsUsable)
            {
                _logger.Error(
                    "Rejecting login of character {CharacterId:X16}: RIN did not return usable character data and {Setting} is off",
                    characterId,
                    nameof(GameServerSettings.AllowHardcodedCharacterData));
                CloseLogin(client);
                return;
            }

            if (data.FetchFailed)
            {
                _logger.Warning("Continuing the login of character {CharacterId:X16} with hardcoded character data", characterId);
            }

            MoveToCharacterShard(client, data, characterId);

            client.AssignedShard.EventBus.Enqueue(new CharacterDataFetchedEvent(client, data));
        }
        finally
        {
            _pending.TryRemove(client.SocketId, out _);
        }
    }

    /// <summary>
    ///     Checks whether the ticket the client presented valid and whether the account behind it may
    ///     play this character. Returns the refusal to close the connection with, or null when the login may go
    ///     ahead.
    ///     The ticket is a long-lived bearer secret covering every character on the account, so it is never
    ///     logged.
    /// </summary>
    /// <param name="ticket">The ticket payload exactly as the client sent it, header included</param>
    /// <param name="characterId">The full character guid the client asked to play</param>
    private static async Task<string> LoginRefusalAsync(byte[] ticket, ulong characterId)
    {
        // There is nothing to check it against.
        if (ticket == null || ticket.Length == 0)
        {
            return "the client presented no login ticket";
        }

        LoginAuthorizationResponse auth;
        try
        {
            auth = await GRPCService.AuthorizeLoginAsync(ticket, characterId);
        }
        catch
        {
            _logger.Error("Could not ask RIN to authorize the login of character {CharacterId:X16}", characterId);
            return "RIN could not be reached to authorize the login";
        }

        if (auth.Authorized)
        {
            _logger.Information(
                "Authorized the login of {CharacterName} ({CharacterId:X16}) on account {AccountId}{Dev}",
                auth.CharacterName,
                characterId,
                auth.AccountId,
                auth.IsDev || auth.CharacterIsDev ? " (developer)" : string.Empty);
            return null;
        }

        if (auth.Reason == _rinDatabaseError)
        {
            _logger.Error("RIN could not check the login of character {CharacterId:X16}: {Reason}", characterId, auth.Reason);
            return "RIN could not check the login";
        }

        // A malformed ticket is not a live secret
        var detail = auth.Reason == _rinTicketMalformed ? $" The client sent {DescribeTicket(ticket)}." : string.Empty;

        _logger.Warning(
            "RIN refused the login of character {CharacterId:X16} on account {AccountId}: {Reason}.{Detail}",
            characterId,
            auth.AccountId,
            auth.Reason,
            detail);
        return $"RIN refused the login ({auth.Reason})";
    }

    private static string DescribeTicket(byte[] ticket)
    {
        var runs = new List<string>();

        var index = 0;
        while (index < ticket.Length)
        {
            var kind = TicketCharKind(ticket[index]);
            var start = index;

            while (index < ticket.Length && TicketCharKind(ticket[index]) == kind)
            {
                index++;
            }

            runs.Add($"{index - start} {kind}");
        }

        var length = ticket.Length == _ticketPayloadLength
                         ? $"{ticket.Length} bytes"
                         : $"{ticket.Length} bytes, a ticket is {_ticketPayloadLength}";

        var header = HasTicketHeader(ticket) ? " with the \"R5 \" header" : string.Empty;

        var shape = runs.Count <= 6
                        ? string.Join(", ", runs)
                        : $"{string.Join(", ", runs.Take(5))}, … ({runs.Count} runs)";

        return $"{length}{header}: {shape}";
    }

    private static bool HasTicketHeader(byte[] ticket)
        => ticket.Length >= 3 && ticket[0] == (byte)'R' && ticket[1] == (byte)'5' && ticket[2] == (byte)' ';

    private static string TicketCharKind(byte b)
    {
        if (b == 0)
        {
            return "NUL";
        }

        if (b is >= 0x21 and <= 0x7E && (char.IsAsciiLetter((char)b) || char.IsAsciiDigit((char)b) || "+/=".Contains((char)b)))
        {
            return "base64";
        }

        if (b == 0x20)
        {
            return "space";
        }

        return b is >= 0x20 and <= 0x7E ? "text" : "binary";
    }

    private static async Task<CharacterData> RetrieveCharacterAsync(ulong characterId)
    {
        var (character, characterFailed) = await FetchAsync(() => GRPCService.GetCharacterAndBattleframeVisualsAsync((long)characterId), "character");
        var (inventory, inventoryFailed) = await FetchAsync(() => GRPCService.GetCharacterInventoryAsync((long)characterId), "character inventory");
        var (unlocks, unlocksFailed) = await FetchAsync(() => GRPCService.GetCharacterUnlocksAsync((long)characterId), "character unlocks");
        var (battleframes, battleframesFailed) = await FetchAsync(() => GRPCService.GetBattleframeProgressionAsync((long)characterId), "character battleframe progression");

        return new CharacterData(character, inventory, unlocks, battleframes)
        {
            FetchFailed = characterFailed || inventoryFailed || unlocksFailed || battleframesFailed,
        };
    }

    private static async Task<(T Value, bool Failed)> FetchAsync<T>(Func<Task<T>> fetch, string what)
        where T : class
    {
        try
        {
            return (await fetch(), false);
        }
        catch
        {
            _logger.ForContext(typeof(GRPCService)).Warning("Could not get the {What} over GRPC", what);
            return (null, true);
        }
    }

    /// <summary>
    ///     Move the connection to the shard of the zone the character belongs in, before anything gets created
    ///     on it. The connection keeps its channels, so it never has to reconnect.
    /// </summary>
    private static void MoveToCharacterShard(NetworkClient client, CharacterData data, ulong characterId)
    {
        var zone = PickZone(characterId, data);
        var shard = _shards.GetOrCreateShard(zone.ID);

        if (shard == client.AssignedShard)
        {
            return;
        }

        var player = (INetworkPlayer)client.Player;

        client.AssignedShard.MigrateOut(player);
        shard.TransferIn(player);
    }

    /// <summary>
    ///     The character's last zone from RIN, or for fallback characters the zone encoded in the low bits of
    ///     the id. Unknown zones fall back to the default shard's zone.
    /// </summary>
    /// <param name="characterId">The character id; its low bits encode the zone for fallback characters.</param>
    /// <param name="data">The RIN character data carrying the last zone id, or null for fallback characters.</param>
    /// <returns>The zone to spawn the character in.</returns>
    private static Zone PickZone(ulong characterId, CharacterData data)
    {
        var zoneId = data.Character != null ? data.Character.CharacterInfo.LastZoneId : (uint)(characterId & 0x000000000000ffff);

        if (DataUtils.TryGetZone(zoneId, out var zone))
        {
            return zone;
        }

        _logger.Information("Zone {ZoneId} isn't known, using the default zone", zoneId);
        return DataUtils.GetZone(_shards.DefaultShard.ZoneId);
    }

    /// <summary>
    ///     Tells the client to close the connection, ending a login that cannot be completed. Called before the
    ///     character is moved onto a shard, so there is nothing on the shard side to undo.
    /// </summary>
    private static void CloseLogin(NetworkClient client)
    {
        var resp = new CloseConnection { ShutdownCode = 0 };
        client.NetChannels[ChannelType.Control].SendMessage(resp);
    }
}
