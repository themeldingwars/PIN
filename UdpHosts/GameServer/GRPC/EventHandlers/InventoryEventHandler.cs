using System.Collections.Generic;
using System.Linq;
using GrpcGameServerAPIClient;
using Serilog;
using CharacterLoadout = GameServer.Data.CharacterLoadout;

namespace GameServer.GRPC.EventHandlers;

public static class InventoryEventHandler
{
    private static readonly ILogger _logger = Log.ForContext(typeof(InventoryEventHandler));

    /// <summary>
    ///     Handles an inventory change RIN reports.
    ///     RIN raises this after every inventory command, including the ones this server just sent, and broadcasts it to
    ///     every connected game server. The server hosting the character owns its in-memory inventory, so re-pulling
    ///     here would race with live state; the character is pulled again at login anyway.
    /// </summary>
    /// <param name="e">The event</param>
    /// <param name="clients">The connected clients</param>
    public static void HandleEvent(InventoryUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        // RIN only fills the guid in on builds with the pg_notify payload fix; on older ones it is 0 and tells us nothing
        if (e.CharacterGuid == 0)
        {
            _logger.Debug("RIN reported an inventory change without a character guid");
            return;
        }

        var hosted = clients.Values.Any(p => p.CharacterId + 0xFE == e.CharacterGuid);
        _logger.Debug(
            "Inventory of {CharacterGuid:X16} changed in RIN, {Action}",
            e.CharacterGuid,
            hosted ? "we caused it and stay authoritative" : "character is not hosted here");
    }

    public static void HandleEvent(LoadoutPerksUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        var player = clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == e.CharacterGuid);
        if (player?.Inventory == null || player.CharacterEntity == null)
        {
            return;
        }

        if (!player.Inventory.SetLoadoutPerks(e.LoadoutId, [.. e.PerkSdbIds]))
        {
            return;
        }

        var character = player.CharacterEntity;
        if (character.SelectedLoadout == e.LoadoutId && character.CurrentLoadout != null)
        {
            var refData = player.Inventory.GetLoadoutReferenceData(e.LoadoutId);
            if (refData != null)
            {
                character.ApplyLoadout(new CharacterLoadout(refData));
            }
        }

        _logger.Information(
            "{Player} locked in {Count} perks on loadout {LoadoutId}: {Perks}",
            player,
            e.PerkSdbIds.Count,
            e.LoadoutId,
            e.PerkSdbIds);
    }
}
