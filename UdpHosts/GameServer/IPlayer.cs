using System.Numerics;
using System.Threading;
using GameServer.Data;
using CharacterEntity = GameServer.Entities.Character.CharacterEntity;

namespace GameServer;

public interface IPlayer
{
    public enum PlayerStatus
    {
        Invalid = -1,
        Unknown = 0,
        Connecting = 1,
        Connected,
        LoggingIn,
        LoggedIn,
        Loading,

        Playing = 999
    }

    ulong PlayerId { get; set; }
    ulong CharacterId { get; set; }
    ulong EntityId => CharacterId & 0xffffffffffffff00; // Ignore last byte
    CharacterEntity CharacterEntity { get; set; }
    CharacterInventory Inventory { get; set; }
    CharacterUnlocks Unlocks { get; set; }
    CharacterBattleframes Battleframes { get; set; }
    PlayerStatus Status { get; set; }
    PlayerPreferences Preferences { get; }
    Zone CurrentZone { get; }
    uint ConnectedAt { get; }
    uint LastRequestedUpdate { get; set; }
    uint RequestedClientTime { get; set; }
    bool FirstUpdateRequested { get; set; }
    bool CanReceiveGSS { get; }

    /// <summary>
    ///     Wait for the client to request its own remote views
    /// </summary>
    bool OwnCharacterViewsRequested { get; set; }

    /// <summary>
    ///     The player's user id on steam
    /// </summary>
    /// <remarks>
    ///     ToDo: Maybe persist the player's steam id?
    ///     ToDo: Maybe Parse and enable/disable stuff according to https://developer.valvesoftware.com/wiki/SteamID
    /// </remarks>
    ulong SteamUserId { get; set; }

    void Init(IShard shard);

    /// <summary>
    ///     Places the character at the spawn point of a zone and tells the client to enter it
    /// </summary>
    /// <param name="zone">The zone to enter</param>
    /// <param name="outpostId">The outpost to spawn at, 0 for the zone's spawn point</param>
    void EnterZone(Zone zone, uint outpostId = 0);
    void EnterZoneAck();
    void ExitZoneAck();
    void Ready();
    void Respawn();
    void Jump();
    void Tick(double deltaTime, ulong currentTime, CancellationToken ct);
    uint FindClosestAvailableOutpost(Zone zone, uint targetOutpostId);
    void RequestRevive(INetworkPlayer reviver);
}