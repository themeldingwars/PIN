using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class AbilitySlottedCommand : Command, ICommand
{
    private AbilitySlottedCommandDef Params;

    public AbilitySlottedCommand(AbilitySlottedCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}