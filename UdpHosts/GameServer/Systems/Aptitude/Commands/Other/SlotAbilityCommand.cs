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

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.AbilityId == 0)
        {
            result.SetPass();
            return;
        }

        foreach (var target in context.Targets)
        {
            if (target is VehicleEntity vehicle)
            {
                vehicle.SlotAbility(Params.AbilityId);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}