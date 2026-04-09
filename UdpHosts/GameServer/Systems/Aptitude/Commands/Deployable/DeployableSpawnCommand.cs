using System.Numerics;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DeployableSpawnCommand : Command, ICommand
{
    public DeployableSpawnCommandDef Params;

    public DeployableSpawnCommand(DeployableSpawnCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;
        var position = target.Position;
        var orientation = Quaternion.Identity;

        if (Params.DeployableTypeId != null && Params.DeployableTypeId != 0)
        {
            var typeId = (uint)Params.DeployableTypeId;
            var entity = context.Shard.EntityMan.SpawnDeployable(typeId, position, orientation);

            if (entity == null)
            {
                Logger.Warning("{Command} {CommandId}, Failed to spawn?", nameof(DeployableSpawnCommand), Params.Id);
                return false;
            }

            if (Params.Lifetime != null && Params.Lifetime != 0)
            {
                context.Shard.EntityMan.SetRemainingLifetime(entity, (uint)Params.Lifetime);
            }

            return true;
        }
        else
        {
            Logger.Warning("Don't know which deployable to spawn in {Command} {CommandId}, failing.", nameof(DeployableSpawnCommand), Params.Id);
            return false;
        }
    }
}