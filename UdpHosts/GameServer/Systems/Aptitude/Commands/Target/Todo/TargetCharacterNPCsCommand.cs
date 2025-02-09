using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class TargetCharacterNPCsCommand : Command, ICommand
{
    private TargetCharacterNPCsCommandDef Params;

    public TargetCharacterNPCsCommand(TargetCharacterNPCsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is not CharacterEntity { IsPlayerControlled: true } player)
        {
            return false;
        }

        context.FormerTargets = new AptitudeTargets(context.Targets);

        // foreach (var npc in player.OwnedNPCs)
        // {
        //     context.Targets.Push(npc);
        // }

        return true;
    }
}