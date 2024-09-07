using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEnergyByRangeCommand : Command, ICommand
{
    private RequireEnergyByRangeCommandDef Params;

    public RequireEnergyByRangeCommand(RequireEnergyByRangeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}