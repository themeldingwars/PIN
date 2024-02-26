using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ResourceNodeBeaconCalldownCommand : ICommand
{
    private ResourceNodeBeaconCalldownCommandDef Params;

    public ResourceNodeBeaconCalldownCommand(ResourceNodeBeaconCalldownCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var caller = context.Self;
        var request = context.Abilities.TryConsumeResourceNodeBeaconCalldownRequest(caller.EntityId);
        if (request != null)
        {
            var entityMan = context.Shard.EntityMan;
            uint nodeType = 20; // TODO: Figure out how to use and determine these
            var beaconType = Params.ResourceNodeBeaconId;
            var position = request.Position;
            entityMan.SpawnThumper(nodeType, beaconType, position);
            return true;
        }
        else
        {
            return false;
        }
    }
}