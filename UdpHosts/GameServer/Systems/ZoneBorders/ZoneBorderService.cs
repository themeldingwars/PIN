using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServer.Entities.Character;
using GameServer.Systems.Aptitude;

namespace GameServer.Systems.ZoneBorders;

/// <summary>
///     Moves players across the border between Coral Forest and Sertao the way the live servers did. Inside the zone's
///     transfer bounds a player gets the zone's transfer effect, "Transferring to Sertao SIN Network…" for two seconds with
///     a beep every second. Leaving the bounds removes it, and the effect's remove chain shows "Transfer cancelled". If it
///     runs out, its remove chain runs TeleportInstance, which moves the player to the same spot in the other zone.
/// </summary>
public sealed class ZoneBorderService
{
    // The transfer effect of each zone with transfer bounds, from the RequireZoneType check in its apply chain
    private static readonly Dictionary<uint, uint> _transferEffects = new()
    {
        [448] = 7213,
        [1030] = 7212,
    };

    private const ulong _checkIntervalMs = 250;

    // Long enough for the remove chain's teleport to start, so a player whose transfer waits for the other zone's shard
    // doesn't get the countdown a second time
    private const ulong _reapplyDelayMs = 4000;

    private readonly IShard _shard;
    private readonly ZoneGeometry _geometry;
    private readonly uint _effectId;
    private readonly Dictionary<ulong, ulong> _appliedAt = new();
    private ulong _nextCheck;

    public ZoneBorderService(IShard shard)
    {
        _shard = shard;
        _geometry = ZoneGeometry.Get(shard.ZoneId, shard.Settings.MapsPath);
        _effectId = _geometry.TransferBounds != null ? _transferEffects.GetValueOrDefault(shard.ZoneId) : 0;
    }

    public void Tick(ulong currentTime)
    {
        if (_effectId == 0 || currentTime < _nextCheck)
        {
            return;
        }

        _nextCheck = currentTime + _checkIntervalMs;

        foreach (var player in _shard.Clients.Values)
        {
            if (player.CharacterEntity is not { Alive: true } character || player.Status != IPlayer.PlayerStatus.Playing)
            {
                continue;
            }

            var inside = _geometry.TransferBounds.Contains(new Vector2(character.Position.X, character.Position.Y));
            var effect = character.GetActiveEffects().FirstOrDefault(active => active?.Effect.Id == _effectId);

            if (inside && effect == null && !IsTransferring(player) && currentTime >= _appliedAt.GetValueOrDefault(player.CharacterId) + _reapplyDelayMs)
            {
                _appliedAt[player.CharacterId] = currentTime;
                _shard.Abilities.DoApplyEffect(_effectId, character, new Context(_shard, character) { InitTime = _shard.CurrentTime });
            }
            else if (!inside && effect != null)
            {
                _shard.Abilities.DoRemoveEffect(effect);
            }
        }

        foreach (var characterId in _appliedAt.Keys.Where(id => _shard.Clients.Values.All(player => player.CharacterId != id)).ToList())
        {
            _appliedAt.Remove(characterId);
        }
    }

    private static bool IsTransferring(INetworkPlayer player) => player is NetworkPlayer { PendingTransfer: not null };
}
