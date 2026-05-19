using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeEnergyOverTimeCommand : Command, ICommand
{
    private ConsumeEnergyOverTimeCommandDef Params;

    public ConsumeEnergyOverTimeCommand(ConsumeEnergyOverTimeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}