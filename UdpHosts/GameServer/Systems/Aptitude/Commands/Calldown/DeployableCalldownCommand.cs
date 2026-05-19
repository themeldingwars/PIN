using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Calldown;

public class DeployableCalldownCommand : Command, ICommand
{
    private DeployableCalldownCommandDef Params;

    public DeployableCalldownCommand(DeployableCalldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var caller = context.Self;
        var request = context.Abilities.TryConsumeDeployableCalldownRequest(caller.EntityId);
        if (request != null)
        {
            var entityMan = context.Shard.EntityMan;
            var typeId = Params.DeployableType;
            var position = request.Position;
            var orientation = request.Rotation;
            entityMan.SpawnDeployable(typeId, position, orientation, caller as CharacterEntity);

            result.SetPass();
            return;
        }
        else
        {
            result.SetFail();
            return;
        }
    }
}