using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetAiTargetCommand : ICommand
{
    private TargetAiTargetCommandDef Params;

    public TargetAiTargetCommand(TargetAiTargetCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}