using System;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
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
                Console.WriteLine($"DeployableSpawnCommand {Params.Id}, Failed to spawn?");
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
            Console.WriteLine($"Don't know which deployable to spawn in DeployableSpawnCommand {Params.Id}, failing.");
            return false;
        }
    }
}