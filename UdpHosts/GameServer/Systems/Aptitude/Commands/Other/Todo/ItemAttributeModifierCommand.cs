using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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