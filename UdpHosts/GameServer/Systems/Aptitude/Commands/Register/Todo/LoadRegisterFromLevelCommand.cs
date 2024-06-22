using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromLevelCommand : ICommand
{
    private LoadRegisterFromLevelCommandDef Params;

    public LoadRegisterFromLevelCommand(LoadRegisterFromLevelCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}