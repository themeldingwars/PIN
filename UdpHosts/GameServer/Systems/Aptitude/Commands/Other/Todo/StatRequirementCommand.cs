using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class StatRequirementCommand : Command, ICommand
{
    private StatRequirementCommandDef Params;

    public StatRequirementCommand(StatRequirementCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}