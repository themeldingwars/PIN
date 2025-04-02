using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Vehicle;

namespace GameServer.Aptitude;

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