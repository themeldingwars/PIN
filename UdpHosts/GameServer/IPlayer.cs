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

    ulong PlayerId { get; }
    ulong CharacterId { get; }
    ulong EntityId => CharacterId & 0xffffffffffffff00; // Ignore last byte
    CharacterEntity CharacterEntity { get; }
    CharacterInventory Inventory { get; set; }
    PlayerStatus Status { get; }
    PlayerPreferences Preferences { get; }
    Zone CurrentZone { get; }
    uint ConnectedAt { get; }
    uint LastRequestedUpdate { get; set; }
    uint RequestedClientTime { get; set; }
    bool FirstUpdateRequested { get; set; }
    bool CanReceiveGSS { get; }

    /// <summary>
    ///     The player's user id on steam
    /// </summary>
    /// <remarks>
    ///     ToDo: Maybe persist the player's steam id?
    ///     ToDo: Maybe Parse and enable/disable stuff according to https://developer.valvesoftware.com/wiki/SteamID
    /// </remarks>
    ulong SteamUserId { get; set; }

    void Init(IShard shard);

    void Login(ulong characterId);
    void EnterZoneAck();
    void ExitZoneAck();
    void Ready();
    void Respawn();
    void Jump();
    void Tick(double deltaTime, ulong currentTime, CancellationToken ct);
    uint FindClosestAvailableOutpost(Zone zone, uint targetOutpostId);
    void HandleFireWeaponProjectile(uint time, Vector3 aim);
}