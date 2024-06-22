using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromModulePowerCommand : ICommand
{
    private LoadRegisterFromModulePowerCommandDef Params;

    public LoadRegisterFromModulePowerCommand(LoadRegisterFromModulePowerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}