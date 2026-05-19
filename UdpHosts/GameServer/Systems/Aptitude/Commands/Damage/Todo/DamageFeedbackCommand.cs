using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class DamageFeedbackCommand : Command, ICommand
{
    private DamageFeedbackCommandDef Params;

    public DamageFeedbackCommand(DamageFeedbackCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}