using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PushRegisterCommand : ICommand
{
    private PushRegisterCommandDef Params;

    public PushRegisterCommand(PushRegisterCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}