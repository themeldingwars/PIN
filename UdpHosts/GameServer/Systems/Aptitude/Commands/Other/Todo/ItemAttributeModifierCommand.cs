using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ItemAttributeModifierCommand : Command, ICommand
{
    private ItemAttributeModifierCommandDef Params;

    public ItemAttributeModifierCommand(ItemAttributeModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}