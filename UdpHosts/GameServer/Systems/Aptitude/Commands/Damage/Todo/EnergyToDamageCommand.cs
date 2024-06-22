using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class EnergyToDamageCommand : ICommand
{
    private EnergyToDamageCommandDef Params;

    public EnergyToDamageCommand(EnergyToDamageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}