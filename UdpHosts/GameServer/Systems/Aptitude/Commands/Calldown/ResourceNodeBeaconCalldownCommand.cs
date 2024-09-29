using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class ResourceNodeBeaconCalldownCommand : Command, ICommand
{
    private ResourceNodeBeaconCalldownCommandDef Params;

    public ResourceNodeBeaconCalldownCommand(ResourceNodeBeaconCalldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var caller = context.Self;
        var request = context.Abilities.TryConsumeResourceNodeBeaconCalldownRequest(caller.EntityId);
        if (request != null)
        {
            var encounterMan = context.Shard.EncounterMan;
            uint nodeType = 20; // TODO: Figure out how to use and determine these
            var position = request.Position;
            encounterMan.CreateThumper(nodeType, position, (BaseEntity)caller, Params);
            return true;
        }
        else
        {
            return false;
        }
    }
}