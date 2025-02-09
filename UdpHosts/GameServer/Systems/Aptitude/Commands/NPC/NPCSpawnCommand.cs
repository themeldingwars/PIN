using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class NPCSpawnCommand : Command, ICommand
{
    private NPCSpawnCommandDef Params;

    public NPCSpawnCommand(NPCSpawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.MonsterId == 0)
        {
            return true;
        }

        var owner = Params.SetOwner ? context.Self as CharacterEntity : null;

        context.Shard.EntityMan.SpawnCharacter(Params.MonsterId, context.InitPosition, owner);

        return true;
    }
}