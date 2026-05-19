using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireMovingCommand : Command, ICommand
{
    private RequireMovingCommandDef Params;

    public RequireMovingCommand(RequireMovingCommandDef par)
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
            if (Params.CheckVelocity == 1)
            {
                if (Params.Velocitytol == 0)
                {
                    cmdResult = !character.MovementStateContainer.Movement;
                }
                else
                {
                    // todo
                    Logger.Debug("[{Command} {CommandId}] velocity tolerance: {VelocityTol}, negate: {Negate}", nameof(RequireMovingCommand), Params.Id, Params.Velocitytol, Params.Negate);
                }
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireMovingCommand), Params.Id);
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