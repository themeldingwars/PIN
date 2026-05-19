using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromDamageCommand : Command, ICommand
{
    private LoadRegisterFromDamageCommandDef Params;

    public LoadRegisterFromDamageCommand(LoadRegisterFromDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}