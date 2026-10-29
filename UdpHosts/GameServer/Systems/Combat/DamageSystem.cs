using System.Threading;
using AeroMessages.GSS.Character;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB;
using GameServer.Systems.Aptitude;
using GameServer.Systems.NpcDeath;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Combat;

public class DamageSystem
{
    private static readonly ILogger _logger = Log.ForContext<DamageSystem>();

    private readonly IEventBus _eventBus;
    private readonly Shard _shard;
    private readonly INpcDeathRules _rules;

    public DamageSystem(IEventBus eventBus, Shard shard, INpcDeathRules rules)
    {
        _eventBus = eventBus;
        _shard = shard;
        _rules = rules;
    }

    public bool ApplyDamage(IEntity target, int amount, IEntity source = null, DamageInfo? damageInfo = null)
    {
        if (target == null || amount <= 0 || !CanTakeDamage(target))
        {
            return false;
        }

        DamageInfo info = damageInfo ?? new DamageInfo(amount, 0, false);

        if (target is CharacterEntity character)
        {
            ApplyDamageToCharacter(character, amount, source, info);
        }
        else if (target is DeployableEntity deployable)
        {
            ApplyDamageToDeployable(deployable, amount, source, info);
        }
        else
        {
            _logger.Warning("ApplyDamage called on non-damageable entity {EntityId}, ignoring", target.EntityId);
            return false;
        }

        _eventBus.Publish(new EntityDamagedEvent(target, amount, source));
        return true;
    }

    public void ApplyHeal(IEntity target, int amount, IEntity source = null)
    {
        if (amount <= 0)
        {
            return;
        }

        if (target is CharacterEntity character)
        {
            ApplyHealToCharacter(character, amount, source);
        }
        else if (target is DeployableEntity deployable)
        {
            ApplyHealToDeployable(deployable, amount, source);
        }
        else
        {
            _logger.Warning("ApplyHeal called on non-damageable entity {EntityId}, ignoring", target.EntityId);
            return;
        }

        _eventBus.Publish(new EntityHealedEvent(target, amount, source));
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }

    private static bool CanTakeDamage(IEntity target)
    {
        return target switch
        {
            CharacterEntity character => character.CharacterState.State != CharacterStateData.CharacterStatus.Dead && character.CurrentHealth > 0,
            DeployableEntity deployable => !deployable.IsDead && deployable.CurrentHealth > 0,
            _ => false,
        };
    }

    private void ApplyDamageToCharacter(CharacterEntity character, int amount, IEntity source, DamageInfo damageInfo)
    {
        int remaining = amount;

        if (character.CurrentShields > 0)
        {
            int shieldAbsorb = int.Min(remaining, character.CurrentShields);
            character.SetCurrentShields(character.CurrentShields - shieldAbsorb);
            remaining -= shieldAbsorb;
        }

        if (remaining > 0)
        {
            character.SetCurrentHealth(character.CurrentHealth - remaining);
        }

        character.SetLastDamage(damageInfo);
    }

    private void ApplyHealToCharacter(CharacterEntity character, int amount, IEntity source)
    {
        character.SetCurrentHealth(character.CurrentHealth + amount);
    }

    private void ApplyDamageToDeployable(DeployableEntity deployable, int amount, IEntity source, DamageInfo damageInfo)
    {
        if (deployable.IsDead)
        {
            return;
        }

        deployable.SetCurrentHealth(deployable.CurrentHealth - amount);

        if (deployable.CurrentHealth > 0)
        {
            return;
        }

        deployable.MarkDead();
        _shard.EntityMan.DetachNpcGunner(deployable);

        var deployableInfo = SDBInterface.GetDeployable(deployable.Type);
        uint gibsetId = deployableInfo?.GibsetId ?? 0;
        if (GibVisualsSelector.TrySelectGibVisualsId(gibsetId, damageInfo, deployable.MaxHealth, deployable.CurrentHealth, out uint gibVisualsId))
        {
            deployable.SetGibVisuals(gibVisualsId);
        }
        else
        {
            _logger.Debug("No gib visuals id available for {Name} gibset={GibsetId}, skipping gib visuals", deployable, gibsetId);
        }

        _logger.Information("{Name} destroyed", deployable);

        _eventBus.Publish(new DeployableDestroyedEvent(deployable, source));

        // The death ability carries what the SDB loot table doesn't, a surface deposit's crystite comes from its SpawnLoot
        if (deployableInfo?.DeathAbilityid is > 0 and var deathAbility)
        {
            var initiator = source as IAptitudeTarget ?? deployable;
            _shard.Abilities.HandleActivateAbility(_shard, initiator, deathAbility, _shard.CurrentTime, new AptitudeTargets(), self: deployable);
        }

        _shard.EntityMan.SetRemainingLifetime(deployable, (uint)_rules.DestroyedDeployableLingerMs);
    }

    private void ApplyHealToDeployable(DeployableEntity deployable, int amount, IEntity source)
    {
        if (deployable.IsDead)
        {
            return;
        }

        deployable.SetCurrentHealth(deployable.CurrentHealth + amount);
    }
}
