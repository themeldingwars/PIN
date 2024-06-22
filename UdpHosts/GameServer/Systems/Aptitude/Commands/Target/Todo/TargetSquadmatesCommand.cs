using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetSquadmatesCommand : ICommand
{
    private TargetSquadmatesCommandDef Params;

    public TargetSquadmatesCommand(TargetSquadmatesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}