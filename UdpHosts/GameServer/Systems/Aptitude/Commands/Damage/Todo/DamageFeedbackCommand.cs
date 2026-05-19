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

    public bool Execute(Context context)
    {
        return true;
    }
}