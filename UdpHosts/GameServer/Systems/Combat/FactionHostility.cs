using System.Linq;
using GameServer.StaticDB;
using Serilog;

namespace GameServer.Systems.Combat;

/// <summary>
///     The faction half of hostility.
///     dbcharacter::Faction.default_stance is the posture a faction takes towards anything, and
///     dbcharacter::FactionRelations rows override individual pairs.
/// </summary>
public class FactionHostility
{
    private static readonly ILogger _logger = Log.ForContext<FactionHostility>();

    // Stance matrix indexed by [sourceFactionId, targetFactionId]. Faction ids are 1 based, row/column 0 is left unused.
    private HostilityStance[,] _stanceMatrix;
    private uint _numFactions;

    public FactionHostility()
    {
        LoadFromSDB();
    }

    /// <summary>
    ///     Maps an SDB stance column (Faction.default_stance, FactionRelations.hostility_stance or
    ///     FactionReputations.hostility_stance) onto the three way hostility the game resolves to. Those columns are
    ///     a reputation style scale - 3 honored, 2 allied, 1 friendly, 0 neutral, -1 hostile, -2 hated - so anything
    ///     positive is friendly and anything negative is hostile.
    /// </summary>
    /// <param name="stance">The raw SDB stance column value (reputation-style scale).</param>
    /// <returns>The three-way hostility the value maps to.</returns>
    public static HostilityStance StanceFromSdb(sbyte stance)
    {
        if (stance > 0)
        {
            return HostilityStance.Friendly;
        }

        return stance < 0 ? HostilityStance.Hostile : HostilityStance.Neutral;
    }

    public void LoadFromSDB()
    {
        var factions = SDBInterface.GetFactions();
        _numFactions = factions.Count == 0 ? 0 : factions.Max(faction => faction.Id);
        _stanceMatrix = new HostilityStance[_numFactions + 1, _numFactions + 1];

        // Every faction starts with their default stance towards everything.
        foreach (var faction in factions)
        {
            for (uint target = 1; target <= _numFactions; target++)
            {
                _stanceMatrix[faction.Id, target] = StanceFromSdb(faction.DefaultStance);
            }
        }

        foreach (var relation in SDBInterface.GetFactionRelations())
        {
            // Faction 0 in the relations table means "no faction". Entities without a faction never reach the
            // matrix (GetFactionStance answers them directly), and the client skips these rows the same way.
            if (relation.FactionA == 0 || relation.FactionB == 0)
            {
                continue;
            }

            if (relation.FactionA > _numFactions || relation.FactionB > _numFactions)
            {
                _logger.Warning(
                    "FactionRelations {FactionA} - {FactionB} references an unknown faction (max {NumFactions})",
                    relation.FactionA,
                    relation.FactionB,
                    _numFactions);
                continue;
            }

            var stance = StanceFromSdb(relation.HostilityStance);
            _stanceMatrix[relation.FactionA, relation.FactionB] = stance;

            if (relation.HostilityBidirectional != 0)
            {
                _stanceMatrix[relation.FactionB, relation.FactionA] = stance;
            }
        }

        _logger.Debug("FactionHostility initialized with {NumFactions} factions", _numFactions);
    }

    /// <summary>
    ///     Stance a faction takes towards another one. Mirrors fsHostility::GetFactionStance, which answers
    ///     Friendly for a missing faction or for a faction compared against itself instead of making every caller
    ///     special case it.
    /// </summary>
    /// <param name="sourceFactionId">The faction taking the stance.</param>
    /// <param name="targetFactionId">The faction the stance is towards.</param>
    /// <returns>The stance the source faction takes towards the target.</returns>
    public HostilityStance GetFactionStance(uint sourceFactionId, uint targetFactionId)
    {
        if (sourceFactionId == 0 || targetFactionId == 0 || sourceFactionId == targetFactionId)
        {
            return HostilityStance.Friendly;
        }

        if (sourceFactionId > _numFactions || targetFactionId > _numFactions)
        {
            _logger.Warning(
                "FactionHostility has no stance for unknown faction pair {SourceFaction} - {TargetFaction}",
                sourceFactionId,
                targetFactionId);
            return HostilityStance.Neutral;
        }

        return _stanceMatrix[sourceFactionId, targetFactionId];
    }
}
