using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class ModifyOwnerResourcesCommand : Command, ICommand
{
    private ModifyOwnerResourcesCommandDef Params;

    public ModifyOwnerResourcesCommand(ModifyOwnerResourcesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.ResourceSdbId == 0 || Params.Quantity == 0)
        {
            return true;
        }

        foreach (var target in context.Targets)
        {
            if (target is not CharacterEntity { IsPlayerControlled: true } character)
            {
                continue;
            }

            if (Params.Quantity > 0)
            {
                character.Player.Inventory.AddResource(Params.ResourceSdbId, (uint)Params.Quantity);
            }
            else
            {
                character.Player.Inventory.ConsumeResource(Params.ResourceSdbId, (uint)(-Params.Quantity));
            }
        }

        return true;
    }
}