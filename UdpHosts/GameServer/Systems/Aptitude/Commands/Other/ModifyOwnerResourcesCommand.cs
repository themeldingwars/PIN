using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ModifyOwnerResourcesCommand : Command, ICommand
{
    private ModifyOwnerResourcesCommandDef Params;

    public ModifyOwnerResourcesCommand(ModifyOwnerResourcesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.ResourceSdbId == 0 || Params.Quantity == 0)
        {
            result.SetPass();
            return;
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

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}