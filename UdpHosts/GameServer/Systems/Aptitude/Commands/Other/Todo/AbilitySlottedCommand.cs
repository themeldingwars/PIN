using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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