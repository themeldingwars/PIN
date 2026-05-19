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

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.MonsterId == 0)
        {
            result.SetPass();
            return;
        }

        var owner = Params.SetOwner ? context.Self as CharacterEntity : null;

        context.Shard.EntityMan.SpawnCharacter(Params.MonsterId, context.InitPosition, owner);

        result.SetPass();
        return;
    }
}