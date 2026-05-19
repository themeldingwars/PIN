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

    public bool Execute(Context context)
    {
        return true;
    }
}