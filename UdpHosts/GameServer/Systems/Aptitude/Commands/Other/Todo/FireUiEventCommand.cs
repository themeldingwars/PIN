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

    public bool Execute(Context context)
    {
        return true;
    }
}