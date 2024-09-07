using GameServer.Data.SDB.Records.aptfs;

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
        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = target as Entities.Character.CharacterEntity;
            result = character.IsAirborne;
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}