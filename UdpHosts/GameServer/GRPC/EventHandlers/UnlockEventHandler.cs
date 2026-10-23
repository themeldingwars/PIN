using System.Collections.Generic;
using System.Linq;
using GrpcGameServerAPIClient;
using Serilog;

namespace GameServer.GRPC.EventHandlers;

public static class UnlockEventHandler
{
    private static readonly ILogger _logger = Log.ForContext(typeof(UnlockEventHandler));

    /// <summary>
    ///     Handles RIN reporting that a character's unlocks changed.
    ///     Unlike InventoryUpdated, this one is raised by a trigger on the unlock table, so it also covers unlocks
    ///     the web API granted - store purchases and GM tooling arrive here. The event carries no detail, so the
    ///     hosted character re-reads its whole unlock set.
    /// </summary>
    /// <param name="e">The event</param>
    /// <param name="clients">The connected clients</param>
    public static void HandleEvent(UnlocksUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        // RIN only fills the guid in on builds with the notify payload fix; on older ones it is 0 and tells us nothing
        if (e.CharacterGuid == 0)
        {
            _logger.Debug("RIN reported an unlock change without a character guid");
            return;
        }

        var player = clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == e.CharacterGuid);
        if (player?.Unlocks == null)
        {
            return;
        }

        _ = player.Unlocks.RefreshAsync();
    }
}
