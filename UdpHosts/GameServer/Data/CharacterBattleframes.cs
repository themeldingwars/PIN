using System.Collections.Generic;
using System.Linq;
using GameServer.Entities.Character;
using GameServer.GRPC;
using Serilog;
using GrpcProgressionResponse = GrpcGameServerAPIClient.BattleframeProgressionResponse;

namespace GameServer.Data;

/// <summary>
/// One upgrade the character has already taken on a battleframe, and how many times.
/// </summary>
/// <param name="UpgradeId">The upgrade within the elite level tables</param>
/// <param name="Count">How many times it was taken</param>
public readonly record struct BattleframeUpgrade(uint UpgradeId, uint Count);

/// <summary>
/// One upgrade currently on offer for a battleframe.
/// </summary>
/// <param name="UpgradeIndex">Position within the rolled set, unique per battleframe</param>
/// <param name="UpgradeId">The upgrade within the elite level tables</param>
/// <param name="Category">What kind of upgrade it is</param>
/// <param name="StatValue">The strength of the upgrade</param>
/// <param name="Count">How many it grants, when the upgrade counts rather than measures</param>
/// <param name="ItemSdbId">The item it grants, when it grants one</param>
/// <param name="LocalizationId">The string table id of its name</param>
public readonly record struct BattleframeAvailableUpgrade(
    uint UpgradeIndex,
    uint UpgradeId,
    uint Category,
    float StatValue,
    uint? Count,
    uint? ItemSdbId,
    uint LocalizationId);

/// <summary>
/// The progression of one battleframe of the character.
/// </summary>
public class BattleframeProgression
{
    public uint ChassisId { get; set; }

    public ulong BattleframeGuid { get; set; }

    public uint Level { get; set; }

    public ulong CurrentXp { get; set; }

    public ulong LifetimeXp { get; set; }

    public uint EliteLevel { get; set; }

    public ulong CurrentEliteXp { get; set; }

    public ulong LifetimeEliteXp { get; set; }

    public uint ElitePoints { get; set; }

    /// <summary>The upgrades the character has spent elite points on.</summary>
    public List<BattleframeUpgrade> PreviousUpgrades { get; set; } = [];

    /// <summary>The upgrades currently rolled and waiting for the character to pick one, empty when nothing is pending.</summary>
    public List<BattleframeAvailableUpgrade> AvailableUpgrades { get; set; } = [];

    public uint Bandwidth { get; set; }

    /// <summary>
    /// Indicates that this is fake data, e.g. a temporary battleframe that is not persisted
    /// </summary>
    public bool Placeholder { get; set; }
}

public class CharacterBattleframes
{
    private static readonly ILogger _logger = Log.ForContext<CharacterBattleframes>();

    private readonly ulong _characterGuid;

    private uint _placeholderLevel = CharacterEntity.DefaultLevel;
    private uint _placeholderEliteLevel = CharacterEntity.DefaultEliteLevel;

    // Replaced wholesale on a load so the GRPC thread never leaves a half-built map for a shard thread to read.
    private Dictionary<uint, BattleframeProgression> _frames; // Keyed by chassis sdb id

    public CharacterBattleframes(ulong characterGuid)
    {
        _characterGuid = characterGuid;
        _frames = [];
    }

    public IReadOnlyDictionary<uint, BattleframeProgression> All => _frames;

    public uint CurrentChassisId { get; private set; }
    public bool CurrentFrameIsPersisted => CurrentChassisId != 0 && IsPersisted(CurrentChassisId);
    public bool IsPersisted(uint chassisId)
        => _frames.TryGetValue(chassisId, out var frame) && !frame.Placeholder;
    public BattleframeProgression Get(uint chassisId)
        => _frames.GetValueOrDefault(chassisId);

    /// <summary>
    /// The progression of one battleframe, creating a placeholder row when the character has none yet.
    /// A frame the character is wearing always has a row, so this only matters for frames that were
    /// never equipped before they started earning XP.
    /// </summary>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis</param>
    /// <returns>The frame's progression</returns>
    public BattleframeProgression GetOrAdd(uint chassisId)
        => Get(chassisId) ?? AddPlaceholderFrame(chassisId);

    /// <summary>
    /// Send the level and xp of one battleframe to RIN. SaveBattleframeXp sets the values absolutely,
    /// so this is the whole row rather than a delta.
    /// </summary>
    /// <param name="chassisId">The dbitems::RootItem sdb_id of the chassis</param>
    public void Persist(uint chassisId)
    {
        var frame = Get(chassisId);

        if (frame == null)
        {
            return;
        }

        if (frame.Placeholder)
        {
            _logger.Debug("Not saving the xp of chassis {ChassisId} for {CharacterId:X16}: the frame is a placeholder",
                chassisId,
                _characterGuid);
            return;
        }

        _ = GRPCService.SendCommandAsync(InventoryCommands.SaveBattleframeXp(_characterGuid, frame));
    }

    /// <summary>
    /// Replace the whole map with the persisted progression of the character. An empty list is valid and means
    /// the character owns no battleframes yet.
    /// </summary>
    /// <param name="response">The persisted progression, or null when it could not be fetched</param>
    public void LoadRemote(GrpcProgressionResponse response)
    {
        if (response == null)
        {
            _placeholderLevel = HardcodedCharacterData.Level;
            _placeholderEliteLevel = HardcodedCharacterData.EliteLevel;

            _logger.Warning(
                "No battleframe progression over GRPC for {CharacterId:X16}; frames without a row play at the hardcoded levels",
                _characterGuid);
            return;
        }

        var frames = new Dictionary<uint, BattleframeProgression>();
        foreach (var entry in response.Progression)
        {
            var chassisId = (uint)entry.BattleframeSdbId;
            frames[chassisId] = new BattleframeProgression()
            {
                ChassisId = chassisId,
                BattleframeGuid = entry.BattleframeGuid,
                Level = entry.Level,
                CurrentXp = entry.CurrentXp,
                LifetimeXp = entry.LifetimeXp,
                EliteLevel = entry.EliteLevel,
                CurrentEliteXp = entry.CurrentEliteXp,
                LifetimeEliteXp = entry.LifetimeEliteXp,
                ElitePoints = entry.ElitePoints,
                Bandwidth = entry.Bandwidth,
                PreviousUpgrades =
                [
                    .. entry.PreviousUpgrades.Select(u => new BattleframeUpgrade(u.UpgradeId, u.Count))
                ],
                AvailableUpgrades =
                [
                    .. entry.AvailableUpgrades.Select(u => new BattleframeAvailableUpgrade(
                        u.UpgradeIndex,
                        u.UpgradeId,
                        u.Category,
                        u.StatValue,
                        u.HasCount ? u.Count : null,
                        u.HasItemSdbId ? u.ItemSdbId : null,
                        u.LocalizationId))
                ],
            };
        }

        _frames = frames;

        _logger.Information(
            "Loaded the progression of {Frames} battleframes over GRPC for {CharacterId:X16}",
            _frames.Count,
            _characterGuid);
    }

    public void ApplyFrameProgression(CharacterEntity character, uint chassisId)
    {
        CurrentChassisId = chassisId;

        var frame = Get(chassisId) ?? AddPlaceholderFrame(chassisId);

        character.ApplyProgressionLevels(frame.Level, frame.EliteLevel);
        character.ApplyProgressionXp(frame.CurrentXp);
    }

    public void ApplyFrameXp(CharacterEntity character, uint chassisId)
    {
        CurrentChassisId = chassisId;

        character.ApplyProgressionXp(Get(chassisId)?.CurrentXp ?? 0);
    }

    private BattleframeProgression AddPlaceholderFrame(uint chassisId)
    {
        var frame = new BattleframeProgression()
        {
            ChassisId = chassisId,
            Level = _placeholderLevel,
            EliteLevel = _placeholderEliteLevel,
            Placeholder = true,
        };

        _frames[chassisId] = frame;
        return frame;
    }
}
