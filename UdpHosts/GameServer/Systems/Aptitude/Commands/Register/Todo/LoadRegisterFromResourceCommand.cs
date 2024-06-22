using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromResourceCommand : ICommand
{
    private LoadRegisterFromResourceCommandDef Params;

    public LoadRegisterFromResourceCommand(LoadRegisterFromResourceCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}