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

    public bool Execute(Context context)
    {
        return true;
    }
}