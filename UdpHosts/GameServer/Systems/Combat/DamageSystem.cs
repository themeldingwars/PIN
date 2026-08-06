using System.Threading;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.StaticDB;
using GameServer.Systems.NpcDeath;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Combat;

public class DamageSystem
{
    private static readonly ILogger Logger = Log.ForContext<DamageSystem>();

    private readonly IEventBus _eventBus;
    private readonly Shard _shard;
    private readonly INpcDeathRules _rules;

    public DamageSystem(IEventBus eventBus, Shard shard, INpcDeathRules rules)
    {
        _eventBus = eventBus;
        _shard = shard;
        _rules = rules;
    }

    public void ApplyDamage(IEntity target, int amount, IEntity source = null)
    {
        if (amount <= 0)
        {
            return;
        }

        if (target is CharacterEntity character)
        {
            ApplyDamageToCharacter(character, amount, source);
        }
        else if (target is DeployableEntity deployable)
        {
            ApplyDamageToDeployable(deployable, amount, source);
        }
        else
        {
            Logger.Warning("ApplyDamage called on non-damageable entity {EntityId}, ignoring", target.EntityId);
            return;
        }

        _eventBus.Publish(new EntityDamagedEvent(target, amount, source));
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
            Logger.Warning("ApplyHeal called on non-damageable entity {EntityId}, ignoring", target.EntityId);
            return;
        }

        _eventBus.Publish(new EntityHealedEvent(target, amount, source));
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }

    private void ApplyDamageToCharacter(CharacterEntity character, int amount, IEntity source)
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
    }

    private void ApplyHealToCharacter(CharacterEntity character, int amount, IEntity source)
    {
        character.SetCurrentHealth(character.CurrentHealth + amount);
    }

    private void ApplyDamageToDeployable(DeployableEntity deployable, int amount, IEntity source)
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

        var deployableInfo = SDBInterface.GetDeployable(deployable.Type);
        deployable.SetGibVisuals(deployableInfo?.GibsetId ?? 0);
        Logger.Information("{Name} destroyed", deployable);

        _shard.EntityMan.SetRemainingLifetime(deployable, (uint)_rules.CorpseLingerMs);
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
