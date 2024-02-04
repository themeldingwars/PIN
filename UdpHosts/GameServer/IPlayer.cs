using System.Threading;
using GameServer.Data;
using CharacterEntity = GameServer.Entities.Character.CharacterEntity;

namespace GameServer;

public interface IPlayer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Still figuring it out")]
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
    PlayerStatus Status { get; }
    Zone CurrentZone { get; }
    uint LastRequestedUpdate { get; set; }
    uint RequestedClientTime { get; set; }
    bool FirstUpdateRequested { get; set; }

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
    void Ready();
    void Respawn();
    void Jump();
    void Tick(double deltaTime, ulong currentTime, CancellationToken ct);
}