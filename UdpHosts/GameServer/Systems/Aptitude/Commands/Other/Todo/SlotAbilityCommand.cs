using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SlotAbilityCommand : ICommand
{
    private SlotAbilityCommandDef Params;

    public SlotAbilityCommand(SlotAbilityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}