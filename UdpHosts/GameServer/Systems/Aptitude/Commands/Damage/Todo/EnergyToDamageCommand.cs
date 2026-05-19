using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

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