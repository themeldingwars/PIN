using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class InflictHitFeedbackCommand : Command, ICommand
{
    private InflictHitFeedbackCommandDef Params;

    public InflictHitFeedbackCommand(InflictHitFeedbackCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}