using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class AirborneDurationCommand : Command, ICommand
{
    private AirborneDurationCommandDef Params;

    public AirborneDurationCommand(AirborneDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self; // NOTE: Investigate

        bool result = false;
        if (target is CharacterEntity character)
        {
            result = character.IsAirborne;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}