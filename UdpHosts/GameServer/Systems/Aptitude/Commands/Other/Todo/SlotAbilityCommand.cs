using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SlotAbilityCommand : Command, ICommand
{
    private SlotAbilityCommandDef Params;

    public SlotAbilityCommand(SlotAbilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}