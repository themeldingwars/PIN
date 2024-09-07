using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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