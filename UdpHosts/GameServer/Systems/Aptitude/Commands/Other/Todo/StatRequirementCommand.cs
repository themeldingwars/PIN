using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class StatRequirementCommand : Command, ICommand
{
    private StatRequirementCommandDef Params;

    public StatRequirementCommand(StatRequirementCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}