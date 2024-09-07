using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ReputationModifierCommand : Command, ICommand
{
    private ReputationModifierCommandDef Params;

    public ReputationModifierCommand(ReputationModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}