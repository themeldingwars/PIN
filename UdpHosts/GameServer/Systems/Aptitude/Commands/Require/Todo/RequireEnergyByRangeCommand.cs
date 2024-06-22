using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEnergyByRangeCommand : ICommand
{
    private RequireEnergyByRangeCommandDef Params;

    public RequireEnergyByRangeCommand(RequireEnergyByRangeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}