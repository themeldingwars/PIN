using System;
using GameServer.Data;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireZoneTypeCommand : Command, ICommand
{
    private RequireZoneTypeCommandDef Params;

    public RequireZoneTypeCommand(RequireZoneTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo aptitude: verify
        bool result = false;

        var target = context.Self;
        if (target is CharacterEntity character)
        {
            var currentZoneId = character.Player.CurrentZone.ID;

            result = (Params.SpecificZoneId != 0 && Params.SpecificZoneId == currentZoneId)
                     || (Params.Holmgang == 1 && Zone.HolmgangZones.Contains(currentZoneId))
                     || (Params.Adventure == 1 && Zone.AdventureZones.Contains(currentZoneId))
                     || (Params.OpenWorld == 1 && Zone.OpenWorldZones.Contains(currentZoneId))
                     || (Params.Other == 1 && Zone.OtherZones.Contains(currentZoneId));
        }
        else
        {
            Console.WriteLine("RequireZoneTypeCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}