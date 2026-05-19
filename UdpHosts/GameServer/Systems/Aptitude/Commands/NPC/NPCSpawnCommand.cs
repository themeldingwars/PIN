using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

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