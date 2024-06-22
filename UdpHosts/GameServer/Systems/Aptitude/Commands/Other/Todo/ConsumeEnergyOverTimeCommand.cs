using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ConsumeEnergyOverTimeCommand : ICommand
{
    private ConsumeEnergyOverTimeCommandDef Params;

    public ConsumeEnergyOverTimeCommand(ConsumeEnergyOverTimeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}