using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByDamageResponseCommand : ICommand
{
    private TargetByDamageResponseCommandDef Params;

    public TargetByDamageResponseCommand(TargetByDamageResponseCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}