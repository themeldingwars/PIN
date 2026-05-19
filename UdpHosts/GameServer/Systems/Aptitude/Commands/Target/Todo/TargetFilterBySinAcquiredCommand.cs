using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetFilterBySinAcquiredCommand : Command, ICommand
{
    private TargetFilterBySinAcquiredCommandDef Params;

    public TargetFilterBySinAcquiredCommand(TargetFilterBySinAcquiredCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}