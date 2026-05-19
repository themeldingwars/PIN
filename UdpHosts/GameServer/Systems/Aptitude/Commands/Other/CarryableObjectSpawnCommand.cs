using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class CarryableObjectSpawnCommand : Command, ICommand
{
    private CarryableObjectSpawnCommandDef Params;

    public CarryableObjectSpawnCommand(CarryableObjectSpawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;
        var position = target.Position;

        if (Params.CarryableTypeId != null && Params.CarryableTypeId != 0)
        {
            var typeId = (uint)Params.CarryableTypeId;
            var entity = context.Shard.EntityMan.SpawnCarryable(typeId, position);

            if (entity == null)
            {
                Logger.Warning("{Command} {CommandId}, Failed to spawn?", nameof(CarryableObjectSpawnCommand), Params.Id);
                result.SetFail();
                return;
            }

            if (Params.Lifetime != null && Params.Lifetime != 0)
            {
                context.Shard.EntityMan.SetRemainingLifetime(entity, (uint)Params.Lifetime);
            }

            result.SetPass();
            return;
        }
        else
        {
            Logger.Warning("Don't know which carryable to spawn in {Command} {CommandId}, failing.", nameof(CarryableObjectSpawnCommand), Params.Id);
            result.SetFail();
            return;
        }
    }

    public override void Reset(Context context)
    {
    }
}