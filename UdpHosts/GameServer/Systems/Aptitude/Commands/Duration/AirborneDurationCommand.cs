using GameServer.Data.SDB.Records.aptfs;
using System;

namespace GameServer.Aptitude;

public class AirborneDurationCommand : ICommand
{
    private AirborneDurationCommandDef Params;

    public AirborneDurationCommand(AirborneDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self; // NOTE: Investigate

        bool result = false;
        if (target.GetType() == typeof(Entities.Character.Character))
        {
            var character = target as Entities.Character.Character;
            result = character.IsAirborne;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}