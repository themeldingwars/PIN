using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrpcGameServerAPIClient;
using Serilog;

namespace GameServer.GRPC.EventHandlers;

/// <summary>
///     This server owns the wallets of the characters it hosts and writes them over RIN's database, so RIN never
///     changes them: it asks for payments (wallet charges) and hands currency over as deliveries to claim.
/// </summary>
public static class WalletEventHandler
{
    private static readonly ILogger _logger = Log.ForContext(typeof(WalletEventHandler));

    // RIN delivers back a payment settled late, so retrying is safe, but not for long
    private static readonly TimeSpan[] _settleRetryDelays = [TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(2)];

    public static void HandleEvent(WalletChargeRequested e, IDictionary<uint, INetworkPlayer> clients)
    {
        var player = FindHosted(clients, e.CharacterGuid);
        if (player == null)
        {
            return;
        }

        // The hardcoded fallback wallet isn't persisted, so it can't pay for anything
        var inventory = player.Inventory;
        var paid = inventory.IsPersisted && inventory.ConsumeResource(e.SdbId, e.Quantity);

        // Retries reuse it, so RIN can tell them from a second payment by the same character hosted twice
        _ = SettleAsync(e, paid, Guid.NewGuid().ToString());
    }

    public static void HandleEvent(CurrencyDeliveriesAvailable e, IDictionary<uint, INetworkPlayer> clients)
    {
        var player = FindHosted(clients, e.CharacterGuid);
        if (player != null)
        {
            _ = ClaimDeliveriesAsync(player);
        }
    }

    public static void HandleEvent(RedBeansUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        foreach (var player in clients.Values.Where(p => p.CharacterEntity != null && p.PlayerId == (ulong)e.AccountId))
        {
            player.CharacterEntity.SetRedBeans(e.Balance);
        }
    }

    // Failures only go to the log, RIN keeps an unclaimed delivery for the next claim
    public static async Task ClaimDeliveriesAsync(INetworkPlayer player)
    {
        var shard = player.AssignedShard;
        var inventory = player.Inventory;

        // Would be lost at logout
        if (shard == null || inventory is not { IsPersisted: true })
        {
            return;
        }

        var characterGuid = inventory.CharacterGuid;

        CurrencyDeliveriesResponse resp;
        try
        {
            resp = await GRPCService.ClaimCurrencyDeliveriesAsync(characterGuid);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Couldn't claim the deliveries of {CharacterGuid:X16}, they wait for the next claim", characterGuid);
            return;
        }

        if (resp.Deliveries.Count == 0)
        {
            return;
        }

        // The inventory outlives a logout long enough for this, and its changes are still persisted then
        shard.Post(() =>
        {
            foreach (var delivery in resp.Deliveries)
            {
                _logger.Information("Delivering {Quantity} of {SdbId} to {CharacterGuid:X16}", delivery.Quantity, delivery.SdbId, characterGuid);
                inventory.AddResource(delivery.SdbId, delivery.Quantity);
            }
        });
    }

    private static async Task SettleAsync(WalletChargeRequested charge, bool paid, string paymentId)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                var resp = await GRPCService.SettleWalletChargeAsync(charge, paid, paymentId);

                _logger.Information(
                    "Charge {Kind}/{Id} of {CharacterGuid:X16}, {Quantity} of {SdbId}, paid {Paid}: kept {Kept}, status {Status}",
                    charge.Kind,
                    charge.Id,
                    charge.CharacterGuid,
                    charge.Quantity,
                    charge.SdbId,
                    paid,
                    resp.Kept,
                    resp.Status);
                return;
            }
            catch (Exception ex) when (attempt < _settleRetryDelays.Length)
            {
                _logger.Warning(ex, "Couldn't settle charge {Kind}/{Id} of {CharacterGuid:X16}, trying again", charge.Kind, charge.Id, charge.CharacterGuid);
                await Task.Delay(_settleRetryDelays[attempt]);
            }
            catch (Exception ex)
            {
                // RIN may have settled it and only the answer got lost, so giving the payment back here could pay twice
                _logger.Error(
                    ex,
                    "Gave up settling charge {Kind}/{Id} of {CharacterGuid:X16}: {Quantity} of {SdbId}, paid {Paid}. A paid charge RIN never settled has to be given back by hand",
                    charge.Kind,
                    charge.Id,
                    charge.CharacterGuid,
                    charge.Quantity,
                    charge.SdbId,
                    paid);
                return;
            }
        }
    }

    private static INetworkPlayer FindHosted(IDictionary<uint, INetworkPlayer> clients, ulong characterGuid)
        => clients.Values.FirstOrDefault(p => p.CharacterEntity != null && p.Inventory != null && p.CharacterId + 0xFE == characterGuid);
}
