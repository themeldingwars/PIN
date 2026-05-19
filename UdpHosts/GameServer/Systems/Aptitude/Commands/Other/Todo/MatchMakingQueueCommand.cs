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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}