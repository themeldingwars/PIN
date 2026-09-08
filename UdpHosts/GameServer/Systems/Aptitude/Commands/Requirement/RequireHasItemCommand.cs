using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHasItemCommand : Command, ICommand
{
    private RequireHasItemCommandDef Params;

    public RequireHasItemCommand(RequireHasItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    /// <summary>
    ///     Requirement ahead of the glider pad launch boosts (Lofty pads and the like) and other
    ///     inventory gated perks. Every target has to hold at least Params.Quantity entries of the
    ///     item; targets that cannot carry items at all cannot satisfy it. Params.Negate mirrors the
    ///     requirement for the "does not have the item" chains.
    /// </summary>
    public bool Execute(Context context)
    {
        bool result = false;

        if (context.Targets.Count > 0)
        {
            result = true;
            foreach (IAptitudeTarget target in context.Targets)
            {
                bool targetResult = target is CharacterEntity character
                                    && character.Player?.Inventory != null
                                    && character.Player.Inventory.CountItemsBySdbId(Params.ItemId) >= Params.Quantity;

                if (!targetResult)
                {
                    result = false;
                    break;
                }
            }
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}
