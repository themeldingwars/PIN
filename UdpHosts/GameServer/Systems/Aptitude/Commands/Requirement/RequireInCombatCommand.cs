using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireInCombatCommand : Command, ICommand
{
    private RequireInCombatCommandDef Params;

    public RequireInCombatCommand(RequireInCombatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is CharacterEntity character && character.IsInCombat() == (Params.Negate == 1))
        {
            result.SetFail(StatusCode.DurationRequirementFailed);
            return;
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}
