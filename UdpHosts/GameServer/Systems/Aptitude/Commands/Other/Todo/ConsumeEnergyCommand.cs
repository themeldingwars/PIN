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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}