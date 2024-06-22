using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RepositionClonesCommand : ICommand
{
    private RepositionClonesCommandDef Params;

    public RepositionClonesCommand(RepositionClonesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}