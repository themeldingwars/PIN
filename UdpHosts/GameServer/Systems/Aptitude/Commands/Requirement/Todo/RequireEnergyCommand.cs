using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEnergyCommand : Command, ICommand
{
    private RequireEnergyCommandDef Params;

    public RequireEnergyCommand(RequireEnergyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}