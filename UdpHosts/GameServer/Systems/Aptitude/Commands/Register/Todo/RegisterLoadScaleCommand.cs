using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class RegisterLoadScaleCommand : Command, ICommand
{
    private RegisterLoadScaleCommandDef Params;

    public RegisterLoadScaleCommand(RegisterLoadScaleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}