using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ResetCooldownsCommand : Command, ICommand
{
    private ResetCooldownsCommandDef Params;

    public ResetCooldownsCommand(ResetCooldownsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}