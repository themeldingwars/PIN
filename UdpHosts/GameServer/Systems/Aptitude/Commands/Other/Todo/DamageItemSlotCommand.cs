using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DamageItemSlotCommand : Command, ICommand
{
    private DamageItemSlotCommandDef Params;

    public DamageItemSlotCommand(DamageItemSlotCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}