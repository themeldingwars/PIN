using System;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CarryableObjectSpawnCommand : Command, ICommand
{
    private CarryableObjectSpawnCommandDef Params;

    public CarryableObjectSpawnCommand(CarryableObjectSpawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;
        var position = target.Position;

        if (Params.CarryableTypeId != null && Params.CarryableTypeId != 0)
        {
            var typeId = (uint)Params.CarryableTypeId;
            var entity = context.Shard.EntityMan.SpawnCarryable(typeId, position);

            if (entity == null)
            {
                Console.WriteLine($"CarryableObjectSpawnCommand {Params.Id}, Failed to spawn?");
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
            Console.WriteLine($"Don't know which carryable to spawn in CarryableObjectSpawnCommand {Params.Id}, failing.");
            return false;
        }
    }
}