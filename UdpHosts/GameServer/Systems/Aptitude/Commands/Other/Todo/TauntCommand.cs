using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TauntCommand : Command, ICommand
{
    private TauntCommandDef Params;

    public TauntCommand(TauntCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}