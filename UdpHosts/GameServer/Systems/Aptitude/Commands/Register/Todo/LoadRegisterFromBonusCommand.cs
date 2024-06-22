using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromBonusCommand : ICommand
{
    private LoadRegisterFromBonusCommandDef Params;

    public LoadRegisterFromBonusCommand(LoadRegisterFromBonusCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}