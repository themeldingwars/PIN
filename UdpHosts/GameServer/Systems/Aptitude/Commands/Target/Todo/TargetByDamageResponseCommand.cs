using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetByDamageResponseCommand : Command, ICommand
{
    private TargetByDamageResponseCommandDef Params;

    public TargetByDamageResponseCommand(TargetByDamageResponseCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}