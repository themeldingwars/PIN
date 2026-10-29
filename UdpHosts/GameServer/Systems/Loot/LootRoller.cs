using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbitems;
using Serilog;

namespace GameServer.Systems.Loot;

/// <summary>
/// Rolls dbitems::LootTable trees.
/// </summary>
public static class LootRoller
{
    // roll_mode 2 (RollAll) and 3 (RollOneByLevel) treat probability as a percentage, the other pick-one
    // modes treat it as a weight.
    public const byte RollAllMode = 2;
    public const byte RollOneByLevelMode = 3;

    private const int _chancePrecision = 10_000; // percentages are compared in basis points
    private const int _maxDepth = 8;
    private const byte _retiredApprovalState = 4;

    private static readonly ILogger Logger = Log.ForContext(typeof(LootRoller));
    private static readonly ConcurrentDictionary<byte, byte> _simplifiedRollModes = [];
    private static readonly ConcurrentDictionary<uint, byte> _unknownRequirementTypes = [];

    public static List<LootRollResult> Roll(uint lootTableId, LootRollContext context, Random rng)
    {
        var results = new List<LootRollResult>();

        RollTable(lootTableId, context, rng, results, 0, new HashSet<uint>());

        return Merge(results);
    }

    private static void RollTable(uint lootTableId, LootRollContext context, Random rng, List<LootRollResult> results, int depth, HashSet<uint> path)
    {
        if (depth > _maxDepth)
        {
            Logger.Debug("Loot table {LootTableId} is nested deeper than {MaxDepth}, stopping", lootTableId, _maxDepth);
            return;
        }

        if (!path.Add(lootTableId))
        {
            Logger.Debug("Loot table {LootTableId} references itself, stopping", lootTableId);
            return;
        }

        var table = SDBInterface.GetLootTable(lootTableId);

        if (table == null)
        {
            Logger.Debug("Loot table {LootTableId} does not exist in the StaticDB", lootTableId);
            path.Remove(lootTableId);
            return;
        }

        var entries = CollectEntries(table, context);

        if (entries.Count > 0)
        {
            if (table.RollMode == RollAllMode)
            {
                foreach (var entry in entries)
                {
                    if (rng.Next(_chancePrecision) < ScalePercent(entry.Probability))
                    {
                        Take(entry, context, rng, results, depth, path);
                    }
                }
            }
            else if (table.RollMode == RollOneByLevelMode)
            {
                var hits = entries.Where(entry => rng.Next(_chancePrecision) < ScalePercent(entry.Probability)).ToList();

                var winner = PickWeighted(hits, rng);

                if (winner != null)
                {
                    Take(winner.Value, context, rng, results, depth, path);
                }
            }
            else
            {
                NoteSimplifiedMode(table);

                var winner = PickWeighted(entries, rng);

                if (winner != null)
                {
                    Take(winner.Value, context, rng, results, depth, path);
                }
            }
        }

        path.Remove(lootTableId);
    }

    private static void Take(LootEntry entry, LootRollContext context, Random rng, List<LootRollResult> results, int depth, HashSet<uint> path)
    {
        if (entry.IsSubTable)
        {
            RollTable(entry.Target, context, rng, results, depth + 1, path);
            return;
        }

        results.Add(new LootRollResult(entry.Target, RollQuantity(entry.MinQuantity, entry.MaxQuantity, rng)));
    }

    private static List<LootEntry> CollectEntries(LootTable table, LootRollContext context)
    {
        var entries = new List<LootEntry>();

        foreach (var dist in SDBInterface.GetLootTableItemDists(table.Id))
        {
            var item = SDBInterface.GetRootItem(dist.ItemdropId);

            if (item == null)
            {
                Logger.Debug("Loot table {LootTableId} dist {DistId} points at {ItemdropId}, which is not a RootItem.sdb_id", table.Id, dist.DistId, dist.ItemdropId);
                continue;
            }

            if ((context.ExcludeRetired && item.ApprovalState == _retiredApprovalState) ||
                !PassesLevelBand(dist.LevelRange, dist.DistId, context) ||
                !PassesRequiredLevel(item, context) ||
                !PassesRequirements(dist.DistId) ||
                !PassesClassFilter(table, item.ClassCertId, context))
            {
                continue;
            }

            entries.Add(new LootEntry(dist.DistId, dist.ItemdropId, false, dist.Probability, dist.MinQuantity, dist.MaxQuantity));
        }

        foreach (var dist in SDBInterface.GetLootTableSubTableDists(table.Id))
        {
            if (SDBInterface.GetLootTable(dist.SubtableId) == null)
            {
                Logger.Debug("Loot table {LootTableId} dist {DistId} references missing sub table {SubtableId}", table.Id, dist.DistId, dist.SubtableId);
                continue;
            }

            if (!PassesLevelBand(dist.LevelRange, dist.DistId, context) || !PassesRequirements(dist.DistId))
            {
                continue;
            }

            entries.Add(new LootEntry(dist.DistId, dist.SubtableId, true, dist.Probability, 0, 0));
        }

        return entries;
    }

    private static bool PassesLevelBand(ushort levelRange, uint distId, LootRollContext context)
    {
        if (levelRange == 0)
        {
            return true;
        }

        var band = SDBInterface.GetLevelBand(levelRange);

        if (band == null)
        {
            Logger.Debug("Loot dist {DistId} uses level_range {LevelRange}, which is not a LevelBand id, leaving the entry enabled", distId, levelRange);
            return true;
        }

        return context.Level >= band.Min && context.Level <= band.Max;
    }

    private static bool PassesRequiredLevel(RootItem item, LootRollContext context)
    {
        if (context.RequiredLevelWindow < 0)
        {
            return true;
        }

        return item.RequiredLevel <= context.Level + context.RequiredLevelWindow;
    }

    private static bool PassesRequirements(uint distId)
    {
        foreach (var requirement in SDBInterface.GetLootTableDistRequirements(distId))
        {
            // Type 5 is the quality tier gate, which is out of scope until quality rolls exist.
            // The other types are not understood at all, so they never filter anything.
            if (requirement.Type != 5 && _unknownRequirementTypes.TryAdd(requirement.Type, 0))
            {
                Logger.Debug("Loot requirement type {Type} on dist {DistId} is not understood, leaving the entry enabled", requirement.Type, distId);
            }
        }

        return true;
    }

    private static bool PassesClassFilter(LootTable table, ushort classCertId, LootRollContext context)
    {
        if (table.ClassSpecific == null || table.ClassSpecific == 0 || context.ClassCertId == 0)
        {
            return true;
        }

        return classCertId == 0 || classCertId == context.ClassCertId;
    }

    private static void NoteSimplifiedMode(LootTable table)
    {
        // 0 and 1 are both a plain weighted pick, 3 is handled above as a chance roll. 4 gates on quality
        // and 5 on requirements, neither of which is implemented yet.
        if ((table.RollMode == 4 || table.RollMode == 5 || table.RollMode > 5) &&
            _simplifiedRollModes.TryAdd(table.RollMode, 0))
        {
            Logger.Debug("Loot table {LootTableId} roll mode {RollMode} is rolled as a plain weighted pick, its gating is not implemented", table.Id, table.RollMode);
        }
    }

    private static LootEntry? PickWeighted(List<LootEntry> entries, Random rng)
    {
        long total = 0;

        foreach (var entry in entries)
        {
            total += Math.Max(0, entry.Probability);
        }

        if (total <= 0)
        {
            return null;
        }

        long roll = rng.NextInt64(total);

        foreach (var entry in entries)
        {
            var weight = Math.Max(0, entry.Probability);

            if (roll < weight)
            {
                return entry;
            }

            roll -= weight;
        }

        return entries[^1];
    }

    private static int ScalePercent(int probability) => Math.Min(_chancePrecision, probability * 100);

    private static uint RollQuantity(ushort minQuantity, ushort maxQuantity, Random rng)
    {
        var low = (uint)minQuantity;
        var high = (uint)maxQuantity;

        if (low == 0 && high == 0)
        {
            return 1;
        }

        if (high < low)
        {
            high = low;
        }

        return low + (uint)rng.Next((int)(high - low) + 1);
    }

    private static List<LootRollResult> Merge(List<LootRollResult> results)
    {
        var merged = new Dictionary<uint, uint>();

        foreach (var result in results)
        {
            merged[result.SdbId] = merged.GetValueOrDefault(result.SdbId) + result.Quantity;
        }

        return [.. merged.Select(pair => new LootRollResult(pair.Key, pair.Value))];
    }

    private readonly record struct LootEntry(
        uint DistId,
        uint Target,
        bool IsSubTable,
        int Probability,
        ushort MinQuantity,
        ushort MaxQuantity);
}
