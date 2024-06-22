using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireEnergyCommand : ICommand
{
    private RequireEnergyCommandDef Params;

    public RequireEnergyCommand(RequireEnergyCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}