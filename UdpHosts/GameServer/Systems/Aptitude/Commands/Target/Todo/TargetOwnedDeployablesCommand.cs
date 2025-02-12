using System;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class TargetOwnedDeployablesCommand : Command, ICommand
{
    private TargetOwnedDeployablesCommandDef Params;

    public TargetOwnedDeployablesCommand(TargetOwnedDeployablesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is not CharacterEntity character)
        {
            Console.WriteLine("TargetOwnedDeployablesCommand fails because Self is not a Character. If this is happening, we should investigate why.");
            return false;
        }

        context.FormerTargets = new AptitudeTargets(context.Targets);

        foreach (var d in character.OwnedDeployables)
        {
            context.Targets.Push(d);
        }

        return true;
    }
}