using System;
using System.Numerics;
using GameServer.Entities;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Combat;

namespace GameServer.Systems.Aptitude.Commands.Damage;

/// <summary>
///     Works out the damage for every target the context is holding, optionally tapered by how far the target
///     stands from the caster, and hands each hit to the combat pass so it gets the same response multiplier and
///     hit feedback a weapon hit gets.
/// </summary>
public class InflictDamageCommand : Command, ICommand
{
    private const float _epsilon = 0.001f;

    private InflictDamageCommandDef Params;

    public InflictDamageCommand(InflictDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.InitTime is null)
        {
            // The client refuses with status 1 when the context has not been initiated.
            Logger.Debug("{Command} {CommandId} fails because the context isn't initiated, returning false", nameof(InflictDamageCommand), Params.Id);
            result.SetFail(StatusCode.Status1);
            return;
        }

        int amount = (int)AbilitySystem.RegistryOp(context.Register, Params.Damagepoints, (Operand)Params.DamagepointsRegop);

        if (Params.Weapondamage == 1)
        {
            amount = (context.DamagePoints * amount) / 100;

            if (context.DamagePoints == 0)
            {
                Logger.Debug("{Command} {CommandId} has Weapondamage set but the context carries no damage points, so the damage comes out at 0", nameof(InflictDamageCommand), Params.Id);
            }
        }

        if (Params.Usedmgdealt == 1)
        {
            Logger.Debug("{Command} {CommandId} has Usedmgdealt set", nameof(InflictDamageCommand), Params.Id);
        }

        byte damageType = Params.DamageType;
        if (Params.Weapondamagetype == 1 && damageType < 2)
        {
            damageType = context.DamageType;
            Logger.Debug("{Command} {CommandId} has Weapondamagetype set, taking the damage type {damageType} from the context", nameof(InflictDamageCommand), Params.Id, damageType);
        }

        float splashRange = AbilitySystem.RegistryOp(context.Register, Params.Splashrange, (Operand)Params.SplashrangeRegop);
        float pointblankRange = Params.Pointblankrange;

        if (Params.UseWeaponRadius == 1)
        {
            splashRange *= context.ScaleMultiplier;
            pointblankRange *= context.ScaleMultiplier;
        }

        // TODO: Frominitiatorpos
        Vector3 sourcePosition = QueryPose.LiveBodyPosition(context);

        foreach (IAptitudeTarget target in context.Targets)
        {
            // The client drops a target whose game object, or whose damageable component, does not resolve.
            if (!context.Shard.Entities.TryGetValue(target.EntityId, out IEntity entity) || entity == null)
            {
                continue;
            }

            float distance = Vector3.Distance(sourcePosition, entity.Position);
            int damage = TaperByDistance(amount, distance, pointblankRange, splashRange);
            if (damage <= 0)
            {
                continue;
            }

            context.Shard.EventBus.Publish(new InflictDamageEvent(entity.EntityId, context.Initiator.EntityId, damage, damageType, DamageOrigin.Ability, context.AbilityId, Params.Id));
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }

    /// <summary>
    ///     Full damage inside Pointblankrange, none once the target is a whole Splashrange
    ///     band beyond that, with Falloff shaping how the middle of the band falls away. A Falloff of 0 means
    ///     distance is not considered at all.
    /// </summary>
    /// <param name="amount">The resolved damage before any taper</param>
    /// <param name="distance">How far the target is from the caster</param>
    /// <param name="pointblankRange">The radius inside which the damage is undiminished</param>
    /// <param name="splashRange">The width of the band over which the damage reaches zero</param>
    /// <returns>The tapered damage</returns>
    private int TaperByDistance(int amount, float distance, float pointblankRange, float splashRange)
    {
        if (Params.Falloff == 0)
        {
            return amount;
        }

        // The client divides by max(Splashrange, 0.001) rather than by the band's remaining width, so the
        // outer edge sits at Pointblankrange + Splashrange.
        float band = MathF.Max(splashRange, _epsilon);
        float fraction = Math.Clamp((distance - pointblankRange) / band, 0f, 1f);

        // Falloff is a whole number exponent; the client multiplies it out in a loop.
        return (int)((1f - MathF.Pow(fraction, Params.Falloff)) * amount);
    }
}
