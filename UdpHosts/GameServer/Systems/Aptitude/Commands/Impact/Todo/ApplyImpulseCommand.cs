using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ApplyImpulseCommand : Command, ICommand
{
    private ApplyImpulseCommandDef Params;

    public ApplyImpulseCommand(ApplyImpulseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}