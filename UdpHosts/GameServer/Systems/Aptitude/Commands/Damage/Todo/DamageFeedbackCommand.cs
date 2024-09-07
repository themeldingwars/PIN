using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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