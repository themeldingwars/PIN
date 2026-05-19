using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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