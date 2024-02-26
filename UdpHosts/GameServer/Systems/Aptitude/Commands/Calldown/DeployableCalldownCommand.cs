using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class DeployableCalldownCommand : ICommand
{
    private DeployableCalldownCommandDef Params;

    public DeployableCalldownCommand(DeployableCalldownCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var caller = context.Self;
        var request = context.Abilities.TryConsumeDeployableCalldownRequest(caller.EntityId);
        if (request != null)
        {
            var entityMan = context.Shard.EntityMan;
            var typeId = Params.DeployableType;
            var position = request.Position;
            var orientation = request.Rotation;
            entityMan.SpawnDeployable(typeId, position, orientation);

            // TODO: Set owner?
            return true;
        }
        else
        {
            return false;
        }
    }
}