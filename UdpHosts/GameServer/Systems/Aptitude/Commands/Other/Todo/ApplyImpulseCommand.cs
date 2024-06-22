using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ApplyImpulseCommand : ICommand
{
    private ApplyImpulseCommandDef Params;

    public ApplyImpulseCommand(ApplyImpulseCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}