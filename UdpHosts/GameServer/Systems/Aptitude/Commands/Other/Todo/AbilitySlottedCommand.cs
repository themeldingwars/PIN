using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AbilitySlottedCommand : ICommand
{
    private AbilitySlottedCommandDef Params;

    public AbilitySlottedCommand(AbilitySlottedCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}