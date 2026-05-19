using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Hostility;

public class ClearHostilityCommand : Command, ICommand
{
    private ClearHostilityCommandDef Params;

    public ClearHostilityCommand(ClearHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}