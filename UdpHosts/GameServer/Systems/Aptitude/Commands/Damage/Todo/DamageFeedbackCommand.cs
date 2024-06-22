using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DamageFeedbackCommand : ICommand
{
    private DamageFeedbackCommandDef Params;

    public DamageFeedbackCommand(DamageFeedbackCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}