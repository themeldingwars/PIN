using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

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