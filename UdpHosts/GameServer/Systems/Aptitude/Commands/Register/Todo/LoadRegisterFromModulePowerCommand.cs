using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromModulePowerCommand : Command, ICommand
{
    private LoadRegisterFromModulePowerCommandDef Params;

    public LoadRegisterFromModulePowerCommand(LoadRegisterFromModulePowerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}