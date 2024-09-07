using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromBonusCommand : Command, ICommand
{
    private LoadRegisterFromBonusCommandDef Params;

    public LoadRegisterFromBonusCommand(LoadRegisterFromBonusCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}