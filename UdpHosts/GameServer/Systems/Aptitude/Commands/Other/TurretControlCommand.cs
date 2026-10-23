using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Turret;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class TurretControlCommand : Command, ICommand
{
    private TurretControlCommandDef Params;

    public TurretControlCommand(TurretControlCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity gunner)
        {
            Logger.Debug("{Command} {CommandId} executed with a non-character Self ({Self}), skipping", nameof(TurretControlCommand), Params.Id, context.Self);
            result.SetPass();
            return;
        }

        var turret = ResolveTurret(context);

        if (context.ExecutionHint == ExecutionHint.RemoveEffect)
        {
            Unbind(gunner, turret, context);
            result.SetPass();
            return;
        }

        if (turret == null)
        {
            Logger.Warning("{Command} {CommandId} found no turret to give to {Gunner}", nameof(TurretControlCommand), Params.Id, gunner);
            result.SetFail();
            return;
        }

        if (turret.IsNpcControlled && turret.NpcGunner != gunner)
        {
            Logger.Warning("{Command} {CommandId} tried to give {Turret} to {Gunner}, but it is already driven by {Existing}", nameof(TurretControlCommand), Params.Id, turret, gunner, turret.NpcGunner);
            result.SetFail();
            return;
        }

        gunner.TurretControlledBy = turret;
        turret.SetNpcGunner(gunner);

        Logger.Debug("{Command} {CommandId} bound {Gunner} to {Turret}", nameof(TurretControlCommand), Params.Id, gunner, turret);
        result.SetPass();
    }

    private void Unbind(CharacterEntity gunner, TurretEntity turret, Context context)
    {
        if (turret != null && turret.NpcGunner == gunner)
        {
            turret.SetNpcGunner(null);
        }

        gunner.TurretControlledBy = null;

        if (gunner.IsHidden)
        {
            context.Shard.EntityMan.Remove(gunner.EntityId);
        }
    }

    private TurretEntity ResolveTurret(Context context)
    {
        foreach (var candidate in context.Targets)
        {
            var turret = FromEntity(candidate);
            if (turret != null)
            {
                return turret;
            }
        }

        return FromEntity(context.Initiator);
    }

    private TurretEntity FromEntity(object entity)
    {
        return entity switch
        {
            TurretEntity turret => turret,
            DeployableEntity deployable => deployable.Turret,
            _ => null,
        };
    }
}
