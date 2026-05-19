using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireMovementFlagsCommand : Command, ICommand
{
    private RequireMovementFlagsCommandDef Params;

    public RequireMovementFlagsCommand(RequireMovementFlagsCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            if (Params.Crouch == 1 && character.MovementStateContainer.Crouch)
            {
                cmdResult = true;
            }
            else if (Params.Sprint == 1 && character.MovementStateContainer.Sprint)
            {
                cmdResult = true;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireMovementFlagsCommand), Params.Id);
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

    public override void Reset(Context context)
    {
        return;
    }
}