using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByHostilityCommand : ICommand
{
    private TargetByHostilityCommandDef Params;

    public TargetByHostilityCommand(TargetByHostilityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}