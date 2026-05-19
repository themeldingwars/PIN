using GameServer.Entities.Vehicle;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SlotAbilityCommand : Command, ICommand
{
    private SlotAbilityCommandDef Params;

    public SlotAbilityCommand(SlotAbilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.AbilityId == 0)
        {
            return true;
        }

        foreach (var target in context.Targets)
        {
            if (target is VehicleEntity vehicle)
            {
                vehicle.SlotAbility(Params.AbilityId);
            }
        }

        return true;
    }
}