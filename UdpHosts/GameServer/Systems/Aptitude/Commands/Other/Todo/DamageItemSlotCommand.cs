using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DamageItemSlotCommand : ICommand
{
    private DamageItemSlotCommandDef Params;

    public DamageItemSlotCommand(DamageItemSlotCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}