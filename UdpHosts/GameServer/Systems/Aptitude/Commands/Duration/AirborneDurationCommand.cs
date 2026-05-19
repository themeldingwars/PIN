using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

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