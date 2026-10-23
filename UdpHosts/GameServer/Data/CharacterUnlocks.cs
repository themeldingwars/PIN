using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.GRPC;
using GameServer.Protocol;
using Serilog;
using GrpcUnlocksResponse = GrpcGameServerAPIClient.CharacterUnlocksResponse;

namespace GameServer.Data;

/// <summary>
/// One unlock: the (unlock_type, unlock_id, frame_id) key it is persisted under.
/// </summary>
/// <param name="UnlockType">The category the unlock belongs to, see <see cref="UnlockTypes"/></param>
/// <param name="UnlockId">The id within that category, usually an sdb id</param>
/// <param name="FrameId">The battleframe the unlock is scoped to, 0 when it is not frame-scoped</param>
public readonly record struct UnlockKey(string UnlockType, uint UnlockId, uint FrameId = 0);

/// <summary>
/// The unlock categories PIN writes. The category is stored as a free-form string and nothing validates it,
/// so every side has to use the same spellings for a grant made on one of them to be visible on the others.
/// </summary>
public static class UnlockTypes
{
    public const string Battleframe = "battleframe";
    public const string Title = "title";
}

public class CharacterUnlocks
{
    private static readonly ILogger _logger = Log.ForContext<CharacterUnlocks>();

    private readonly INetworkClient _player;
    private readonly CharacterEntity _character;
    private readonly ulong _characterGuid;

    private HashSet<UnlockKey> _unlocks;

    public CharacterUnlocks(INetworkClient player, CharacterEntity character, ulong characterGuid)
    {
        _player = player;
        _character = character;
        _characterGuid = characterGuid;
        _unlocks = [];
    }

    public IReadOnlyCollection<UnlockKey> All => _unlocks;

    /// <summary>
    /// Whether the character has this exact unlock. FrameId has to match: an unlock scoped to a battleframe
    /// is not the same row as the unscoped one.
    /// </summary>
    /// <param name="unlockType">The category of the unlock</param>
    /// <param name="unlockId">The id within that category</param>
    /// <param name="frameId">The battleframe the unlock is scoped to</param>
    /// <returns>True when the character has it</returns>
    public bool Has(string unlockType, uint unlockId, uint frameId = 0)
        => _unlocks.Contains(new UnlockKey(unlockType, unlockId, frameId));

    /// <summary>
    /// Replace the set with the persisted unlocks of the character. An empty list is valid and means the
    /// character has no unlocks.
    /// </summary>
    /// <param name="response">The persisted unlocks, or null when they could not be fetched</param>
    public void LoadRemote(GrpcUnlocksResponse response)
    {
        if (response == null)
        {
            return;
        }

        _unlocks = [.. response.Unlocks.Select(u => new UnlockKey(u.UnlockType, u.UnlockId, u.FrameId))];

        _logger.Information(
            "Loaded {Unlocks} unlocks over GRPC for {CharacterId:X16}",
            _unlocks.Count,
            _characterGuid);
    }

    /// <summary>
    /// Grant an unlock and persist it. The write is idempotent, so resending one is harmless.
    /// </summary>
    /// <param name="unlockType">The category of the unlock</param>
    /// <param name="unlockId">The id within that category</param>
    /// <param name="frameId">The battleframe the unlock is scoped to</param>
    public void Grant(string unlockType, uint unlockId, uint frameId = 0)
    {
        var unlock = new UnlockKey(unlockType, unlockId, frameId);
        _unlocks.Add(unlock);
        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveUnlock(_characterGuid, unlock));
    }

    /// <summary>
    /// Revoke an unlock and persist it. The delete matches the full key including the frame id and has no
    /// wildcard, so an empty category or a 0 id matches nothing and is silently ignored.
    /// </summary>
    /// <param name="unlockType">The category of the unlock</param>
    /// <param name="unlockId">The id within that category</param>
    /// <param name="frameId">The battleframe the unlock is scoped to</param>
    public void Revoke(string unlockType, uint unlockId, uint frameId = 0)
    {
        var unlock = new UnlockKey(unlockType, unlockId, frameId);
        _unlocks.Remove(unlock);
        _ = GRPCService.SendCommandAsync(InventoryCommands.DeleteUnlock(_characterGuid, unlock));
    }

    /// <summary>
    /// Re-read the persisted unlocks. The change event is raised by a trigger on the storage, so it also covers
    /// unlocks granted elsewhere, and it only says something changed - the whole set has to be re-read.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            LoadRemote(await GRPCService.GetCharacterUnlocksAsync((long)_characterGuid));
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Could not refresh the unlocks of {CharacterId:X16} over GRPC", _characterGuid);
        }
    }

    // TODO: Fixme
    public void SendFullUnlocks()
    {
        var update = new UnlocksUpdate()
        {
            ClearExistingData = 1,
            Groups = []
        };

        _player.NetChannels[ChannelType.ReliableGss].SendMessage(update, _character.EntityId);
    }
}
