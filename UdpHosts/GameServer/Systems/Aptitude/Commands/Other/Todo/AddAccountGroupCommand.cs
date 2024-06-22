using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddAccountGroupCommand : ICommand
{
    private AddAccountGroupCommandDef Params;

    public AddAccountGroupCommand(AddAccountGroupCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}