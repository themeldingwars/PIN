using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetSquadmatesCommand : Command, ICommand
{
    private TargetSquadmatesCommandDef Params;

    public TargetSquadmatesCommand(TargetSquadmatesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}