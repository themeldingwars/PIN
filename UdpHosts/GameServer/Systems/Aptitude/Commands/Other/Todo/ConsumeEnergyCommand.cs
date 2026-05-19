using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeEnergyCommand : Command, ICommand
{
    private ConsumeEnergyCommandDef Params;

    public ConsumeEnergyCommand(ConsumeEnergyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}