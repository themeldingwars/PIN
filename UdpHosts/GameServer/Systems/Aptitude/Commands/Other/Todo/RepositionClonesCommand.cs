using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RepositionClonesCommand : Command, ICommand
{
    private RepositionClonesCommandDef Params;

    public RepositionClonesCommand(RepositionClonesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}