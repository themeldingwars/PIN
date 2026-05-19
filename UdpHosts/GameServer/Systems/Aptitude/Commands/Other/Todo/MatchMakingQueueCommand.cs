using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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