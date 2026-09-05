using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class FireProjectileCommand : Command, ICommand
{
    private readonly FireProjectileCommandDef Params;

    public FireProjectileCommand(FireProjectileCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity character)
        {
            Logger.Warning("{Command} {CommandId} does nothing because self is not a Character. Self is {SourceType}", nameof(FireProjectileCommand), Params.Id, context.Self.GetType().Name);
            result.SetPass();
            return;
        }

        // TODO: Special handling of the register here
        float damage = AbilitySystem.RegistryOp(context.Register, Params.Damage, (Operand)Params.DamageRegop);
        float range = AbilitySystem.RegistryOp(context.Register, Params.Range, (Operand)Params.RangeRegop);

        character.EnqueueAbilityProjectile(new PendingAbilityProjectile
        {
            AmmoType = Params.Ammotype,
            Damage = damage,
            Range = range,
            Spread = Params.Spread,
            BurstCount = Params.Burstcount,
            Hardpoint = Params.Hardpoint,
            AimAtTarget = Params.AimAtTarget != 0,
            UseHomingTarget = Params.UseHomingTarget != 0,
            UseWeaponDamage = Params.UseWeaponDamage != 0,
            ActivationTime = context.InitTime,
            ExecutionId = context.ExecutionId,
            QueueTime = context.Shard.CurrentTime
        });

        Logger.Information(
            "{Command} {CommandId} queued ability projectile ammo={AmmoType} damage={Damage} range={Range} spread={Spread} burst={Burst} for entity={Entity} exec={ExecId}",
            nameof(FireProjectileCommand),
            Params.Id,
            Params.Ammotype,
            damage,
            range,
            Params.Spread,
            Params.Burstcount,
            character.EntityId,
            context.ExecutionId);

        result.SetPass();
    }
}
