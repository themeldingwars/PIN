using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

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