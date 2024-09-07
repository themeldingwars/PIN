using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromLevelCommand : Command, ICommand
{
    private LoadRegisterFromLevelCommandDef Params;

    public LoadRegisterFromLevelCommand(LoadRegisterFromLevelCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}