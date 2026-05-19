using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromStatCommand : Command, ICommand
{
    private LoadRegisterFromStatCommandDef Params;

    public LoadRegisterFromStatCommand(LoadRegisterFromStatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}