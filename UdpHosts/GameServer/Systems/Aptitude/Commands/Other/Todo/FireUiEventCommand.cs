using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class FireUiEventCommand : Command, ICommand
{
    private FireUiEventCommandDef Params;

    public FireUiEventCommand(FireUiEventCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}