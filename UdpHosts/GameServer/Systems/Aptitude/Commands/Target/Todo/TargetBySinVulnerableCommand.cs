using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetBySinVulnerableCommand : Command, ICommand
{
    private TargetBySinVulnerableCommandDef Params;

    public TargetBySinVulnerableCommand(TargetBySinVulnerableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}