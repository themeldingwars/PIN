using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class RegisterLoadScaleCommand : ICommand
{
    private RegisterLoadScaleCommandDef Params;

    public RegisterLoadScaleCommand(RegisterLoadScaleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}