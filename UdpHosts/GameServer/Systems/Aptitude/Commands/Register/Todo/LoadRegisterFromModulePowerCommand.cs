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

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}