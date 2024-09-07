using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetVisualInfoIndexCommand : Command, ICommand
{
    private SetVisualInfoIndexCommandDef Params;

    public SetVisualInfoIndexCommand(SetVisualInfoIndexCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}