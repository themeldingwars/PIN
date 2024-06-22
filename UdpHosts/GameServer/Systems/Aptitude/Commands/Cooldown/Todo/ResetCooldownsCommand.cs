using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ResetCooldownsCommand : ICommand
{
    private ResetCooldownsCommandDef Params;

    public ResetCooldownsCommand(ResetCooldownsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}