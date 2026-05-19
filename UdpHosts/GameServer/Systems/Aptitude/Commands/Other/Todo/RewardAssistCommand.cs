using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RewardAssistCommand : Command, ICommand
{
    private RewardAssistCommandDef Params;

    public RewardAssistCommand(RewardAssistCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}