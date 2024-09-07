using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RepositionClonesCommand : Command, ICommand
{
    private RepositionClonesCommandDef Params;

    public RepositionClonesCommand(RepositionClonesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}