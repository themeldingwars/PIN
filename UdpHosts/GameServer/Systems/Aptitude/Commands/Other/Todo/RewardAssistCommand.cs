using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RewardAssistCommand : Command, ICommand
{
    private RewardAssistCommandDef Params;

    public RewardAssistCommand(RewardAssistCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}