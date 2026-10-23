using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Protocol;
using Serilog;
using DeployableTookHit = AeroMessages.GSS.Deployable.Events.TookHit;

namespace GameServer.Systems.Combat;

public class HitFeedback
{
    private readonly Shard _shard;
    private readonly ILogger _logger;
    private readonly EntityManager.EntityManager _entityMan;

    public HitFeedback(Shard shard)
    {
        _shard = shard;
        _entityMan = shard.EntityMan;
        _logger = shard.Logger.ForContext<HitFeedback>();
    }

    /// <summary>
    ///     Sends the feedback a damage instance produces: the dealer gets a DealtHit so it can show its own
    ///     numbers, and everyone who has the target scoped in gets the target's TookHit.
    /// </summary>
    /// <param name="target">The entity that took the hit</param>
    /// <param name="source">The entity that dealt it</param>
    /// <param name="damage">The damage after the response multiplier</param>
    /// <param name="damageType">The dbcharacter::DamageType id the hit was dealt with</param>
    /// <param name="headshot">Whether the hit landed on a head hit shape</param>
    /// <param name="crit">Whether the hit was a critical</param>
    public void SendHitFeedback(IEntity target, IEntity source, int damage, byte damageType, bool headshot, bool crit)
    {
        // Build feedback
        DamageResponseFlags damageFlags = 0;

        if (headshot || crit)
        {
            damageFlags |= DamageResponseFlags.Critical;
        }

        DamageHitStruct damageData = new()
        {
            Target = target.AeroEntityId,
            HaveDealer = (byte)(source != null ? 1 : 0),
            Dealer = source != null ? source.AeroEntityId : new EntityId(),
            DamageValue = damage,

            // The struct carries the dbcharacter::DamageType id, which is what lets the client pick the right
            // damage number colour and hit sound for the hit.
            DamageType = damageType,
        };

        // Player Dealt Hit Feedback
        if (source is CharacterEntity sourceCharacter && sourceCharacter.IsPlayerControlled)
        {
            var player = sourceCharacter.Player;
            player.NetChannels[ChannelType.ReliableGss].SendMessage(new DealtHit
            {
                HaveDamage = 1,
                DamageData = damageData,
                DamageFlags = damageFlags,
            },
                sourceCharacter.EntityId);
        }

        // Target Took Hit Feedback. ViewKickScale is the camera kick multiplier the client scales by
        // (value x 10 / 255); nothing drives it per hit yet, so it stays at none.
        if (target is CharacterEntity)
        {
            _entityMan.SendToScoped(target,
                new TookHit
                {
                    HaveDamage = 1,
                    DamageData = damageData,
                    DamageFlags = damageFlags,
                    ShortTime = _shard.CurrentShortTime,
                    ViewKickScale = 0,
                });
        }
        else if (target is DeployableEntity)
        {
            // The deployable TookHit carries the same fields, and the client never reads its ViewKickScale.
            _entityMan.SendToScoped(target,
                new DeployableTookHit
                {
                    HaveDamage = 1,
                    DamageData = damageData,
                    DamageFlags = damageFlags,
                    ShortTime = _shard.CurrentShortTime,
                });
        }
    }
}
