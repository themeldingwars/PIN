using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ConsumeSuperChargeCommand : ICommand
{
    private ConsumeSuperChargeCommandDef Params;

    public ConsumeSuperChargeCommand(ConsumeSuperChargeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}