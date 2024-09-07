using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddAppendageHealthPoolCommand : Command, ICommand
{
    private AddAppendageHealthPoolCommandDef Params;

    public AddAppendageHealthPoolCommand(AddAppendageHealthPoolCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}