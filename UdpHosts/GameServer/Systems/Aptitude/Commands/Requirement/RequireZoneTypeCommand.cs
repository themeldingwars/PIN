using GameServer.Data;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireZoneTypeCommand : Command, ICommand
{
    private RequireZoneTypeCommandDef Params;

    public RequireZoneTypeCommand(RequireZoneTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // todo aptitude: verify
        bool cmdResult = false;

        var target = context.Self;
        if (target is CharacterEntity character)
        {
            var currentZoneId = character.Player.CurrentZone.ID;

            cmdResult = (Params.SpecificZoneId != 0 && Params.SpecificZoneId == currentZoneId)
                     || (Params.Holmgang == 1 && Zone.HolmgangZones.Contains(currentZoneId))
                     || (Params.Adventure == 1 && Zone.AdventureZones.Contains(currentZoneId))
                     || (Params.OpenWorld == 1 && Zone.OpenWorldZones.Contains(currentZoneId))
                     || (Params.Other == 1 && Zone.OtherZones.Contains(currentZoneId));
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireZoneTypeCommand), Params.Id);
        }

        if (Params.Negate == 1)
        {
            cmdResult = !cmdResult;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}