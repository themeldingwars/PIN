using System;
using System.Linq;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEquippedItemCommand : Command, ICommand
{
    private RequireEquippedItemCommandDef Params;

    public RequireEquippedItemCommand(RequireEquippedItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity { CurrentLoadout: { } loadout })
        {
            result.SetFail(StatusCode.Error);
            return;
        }

        var itemId = Params.ItemId;
        var equipped = loadout.ChassisID == itemId
            || loadout.BackpackID == itemId
            || loadout.SlottedItems.Any(slot => slot.Value == itemId || Array.IndexOf(loadout.GetSlotModules(slot.Key), itemId) >= 0)
            || Array.IndexOf(loadout.Perks, itemId) >= 0;

        if (equipped == (Params.Negate != 0))
        {
            result.SetFail(StatusCode.Error);
            return;
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}
