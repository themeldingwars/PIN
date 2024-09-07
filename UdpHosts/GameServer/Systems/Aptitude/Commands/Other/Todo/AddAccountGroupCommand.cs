using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddAccountGroupCommand : Command, ICommand
{
    private AddAccountGroupCommandDef Params;

    public AddAccountGroupCommand(AddAccountGroupCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}