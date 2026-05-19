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

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self; // NOTE: Investigate

        bool cmdResult = false;
        if (target is CharacterEntity character)
        {
            cmdResult = character.IsAirborne;
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
}