using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LoadRegisterFromDamageCommand : ICommand
{
    private LoadRegisterFromDamageCommandDef Params;

    public LoadRegisterFromDamageCommand(LoadRegisterFromDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}