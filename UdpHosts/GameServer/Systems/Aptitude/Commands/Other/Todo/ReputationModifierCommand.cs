using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReputationModifierCommand : ICommand
{
    private ReputationModifierCommandDef Params;

    public ReputationModifierCommand(ReputationModifierCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}