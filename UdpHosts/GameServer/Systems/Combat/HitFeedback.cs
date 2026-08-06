using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using Serilog;

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

    public void TookDebugHit(IEntity target, IEntity source, int damage, bool headshot, bool crit)
    {
        // Build feedback
        DamageResponseFlags damageFlags = 0;

        if (headshot || crit)
        {
            damageFlags |= DamageResponseFlags.Critical;
        }

        ushort shortTime = _shard.CurrentShortTime;
        byte unk2 = 0;
        DamageHitStruct damageData = new()
        {
            Target = target.AeroEntityId,
            HaveDealer = (byte)(source != null ? 1 : 0),
            Dealer = source != null ? source.AeroEntityId : new EntityId(),
            DamageValue = damage,
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

        // Target Took Hit Feedback
        if (target is CharacterEntity || target is DeployableEntity)
        {
            _entityMan.SendToScoped(target,
                new TookHit
                {
                    HaveDamage = 1,
                    DamageData = damageData,
                    DamageFlags = damageFlags,
                    ShortTime = shortTime,
                    Unk2 = unk2,
                });
        }
    }

    public void TookWeaponHit()
    {
    }

    public void TookAbilityHit()
    {
    }

    public void TookCollisionHit()
    {
    }
}