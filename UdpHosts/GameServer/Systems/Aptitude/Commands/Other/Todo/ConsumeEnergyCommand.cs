using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ConsumeEnergyCommand : ICommand
{
    private ConsumeEnergyCommandDef Params;

    public ConsumeEnergyCommand(ConsumeEnergyCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}