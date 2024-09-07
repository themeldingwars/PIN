using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MatchMakingQueueCommand : Command, ICommand
{
    private MatchMakingQueueCommandDef Params;

    public MatchMakingQueueCommand(MatchMakingQueueCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}