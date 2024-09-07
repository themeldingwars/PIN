using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class EnergyToDamageCommand : Command, ICommand
{
    private EnergyToDamageCommandDef Params;

    public EnergyToDamageCommand(EnergyToDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}