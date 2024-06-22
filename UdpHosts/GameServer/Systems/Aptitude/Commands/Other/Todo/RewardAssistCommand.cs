using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RewardAssistCommand : ICommand
{
    private RewardAssistCommandDef Params;

    public RewardAssistCommand(RewardAssistCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}