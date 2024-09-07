using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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