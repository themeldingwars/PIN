using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

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