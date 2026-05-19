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

    public bool Execute(Context context)
    {
        return true;
    }
}