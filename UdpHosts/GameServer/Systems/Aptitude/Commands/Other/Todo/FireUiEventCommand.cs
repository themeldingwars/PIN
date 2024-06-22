using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class FireUiEventCommand : ICommand
{
    private FireUiEventCommandDef Params;

    public FireUiEventCommand(FireUiEventCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}