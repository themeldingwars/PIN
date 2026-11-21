using System;
using System.Globalization;
using System.Linq;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Data;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.dbelitelevels;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Set elite upgrades and bonuses on your current battleframe, in memory only", "elite list [all] | elite set <upgrade id|name> <level> | elite add <category|attribute> <value> | elite clear", "elite")]
public class EliteServerCommand : ProgressionServerCommand
{
    private const string Usage = "Usage: elite list [all] | elite set <upgrade id|name> <level> | elite add <category|attribute> <value> | elite clear";

    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var (success, player) = ResolveSourcePlayer(context);
        if (!success)
        {
            return;
        }

        var character = player.CharacterEntity;
        var progression = player.Battleframes?.Get(character.CurrentLoadout?.ChassisID ?? 0);
        if (progression == null)
        {
            SourceFeedback("No progression for the current battleframe", context);
            return;
        }

        switch (parameters.FirstOrDefault()?.ToLowerInvariant())
        {
            case "list":
                if (parameters.Length > 1 && parameters[1].Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    ListAll(context);
                }
                else
                {
                    List(player, progression, context);
                }

                return;
            case "set":
                if (!Set(parameters, progression, context))
                {
                    return;
                }

                break;
            case "add":
                if (!Add(parameters, player, context))
                {
                    return;
                }

                break;
            case "clear":
                progression.PreviousUpgrades.Clear();
                context.Service.ClearEliteBonuses(player);
                SourceFeedback("Cleared all elite upgrades and bonuses", context);
                break;
            default:
                SourceFeedback(Usage, context);
                return;
        }

        character.ApplyLoadout(character.CurrentLoadout);
        SendUpgradesChanged(player, progression);
        List(player, progression, context);
    }

    private static EliteLevelsUpgradeTable FindUpgrade(string query)
    {
        if (uint.TryParse(query, out var id))
        {
            return SDBInterface.GetEliteLevelsUpgrade(id);
        }

        var normalized = Normalize(query);
        return SDBInterface.GetEliteLevelsUpgrades().FirstOrDefault(upgrade => Normalize(upgrade.Name) == normalized);
    }

    private static string Normalize(string value) => new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static string FormatTarget(EliteLevelsUpgradeTable upgrade)
    {
        if (upgrade.AttributeCategory != 0)
        {
            return $"category {upgrade.AttributeCategory}";
        }

        return upgrade.ItemAttribute != 0 ? $"attribute {upgrade.ItemAttribute}" : "no stat";
    }

    private static void SendUpgradesChanged(INetworkPlayer player, BattleframeProgression progression)
    {
        player.NetChannels[ChannelType.ReliableGss].SendMessage(
            new EliteLevels_UpgradesChanged
            {
                ElitePoints = progression.ElitePoints,
                PreviousUpgrades = [.. progression.PreviousUpgrades.Select(upgrade => new ElitePreviousUpgradeInfo { UpgradeId = upgrade.UpgradeId, Count = upgrade.Count })],
                Success = 1,
            },
            player.CharacterEntity.EntityId);
    }

    private bool Set(string[] parameters, BattleframeProgression progression, ServerCommandContext context)
    {
        if (parameters.Length < 3 || !uint.TryParse(parameters[^1], out var level))
        {
            SourceFeedback("Usage: elite set <upgrade id|name> <level>", context);
            return false;
        }

        var query = string.Join(' ', parameters[1..^1]);
        var upgrade = FindUpgrade(query);
        if (upgrade == null)
        {
            SourceFeedback($"Unknown elite upgrade: {query}, see elite list all", context);
            return false;
        }

        var maxLevel = EliteBonuses.GetMaxLevel(upgrade);
        if (level > maxLevel)
        {
            SourceFeedback($"{upgrade.Name} goes up to level {maxLevel}", context);
            return false;
        }

        progression.PreviousUpgrades.RemoveAll(taken => taken.UpgradeId == upgrade.Id);
        if (level > 0)
        {
            progression.PreviousUpgrades.Add(new BattleframeUpgrade(upgrade.Id, level));
        }

        SourceFeedback($"Set {upgrade.Name} ({upgrade.Id}) to level {level}", context);
        return true;
    }

    private bool Add(string[] parameters, INetworkPlayer player, ServerCommandContext context)
    {
        if (parameters.Length != 3 || !ushort.TryParse(parameters[1], out var id) || !float.TryParse(parameters[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            SourceFeedback("Usage: elite add <category|attribute> <value>", context);
            return false;
        }

        if (!EliteBonuses.IsCategory(id) && SDBInterface.GetAttributeDefinition(id) == null)
        {
            SourceFeedback($"{id} is neither an attribute category nor an item attribute", context);
            return false;
        }

        context.Service.AddEliteBonus(player, id, value);
        SourceFeedback($"Added {value.ToString(CultureInfo.InvariantCulture)} to {(EliteBonuses.IsCategory(id) ? "category" : "attribute")} {id}", context);
        return true;
    }

    private void List(INetworkPlayer player, BattleframeProgression progression, ServerCommandContext context)
    {
        var upgrades = progression.PreviousUpgrades
            .Select(taken => (taken, upgrade: SDBInterface.GetEliteLevelsUpgrade(taken.UpgradeId)))
            .Select(pair => pair.upgrade == null
                ? $"{pair.taken.UpgradeId} x{pair.taken.Count}"
                : $"{pair.upgrade.Name} ({pair.upgrade.Id}) level {pair.taken.Count}: {EliteBonuses.GetUpgradeBonus(pair.upgrade, pair.taken.Count).ToString("0.###", CultureInfo.InvariantCulture)} to {FormatTarget(pair.upgrade)}");
        var extras = (context.Service.GetEliteBonuses(player) ?? new System.Collections.Generic.Dictionary<ushort, float>())
            .Select(pair => $"{(EliteBonuses.IsCategory(pair.Key) ? "category" : "attribute")} {pair.Key}: {pair.Value.ToString("0.###", CultureInfo.InvariantCulture)}");

        var lines = upgrades.Concat(extras).ToList();
        SourceFeedback(lines.Count == 0 ? "No elite upgrades or bonuses" : $"Elite rank {player.CharacterEntity.EliteLevel}, {progression.ElitePoints} points\n{string.Join('\n', lines)}", context);
    }

    private void ListAll(ServerCommandContext context)
    {
        var lines = SDBInterface.GetEliteLevelsUpgrades()
            .OrderBy(upgrade => upgrade.Id)
            .Select(upgrade =>
            {
                var maxLevel = EliteBonuses.GetMaxLevel(upgrade);
                return $"{upgrade.Id} {upgrade.Name}: {FormatTarget(upgrade)}, max level {(maxLevel == uint.MaxValue ? "unlimited" : maxLevel)}";
            });
        SourceFeedback(string.Join('\n', lines), context);
    }
}
