using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireMovingCommand : Command, ICommand
{
    private RequireMovingCommandDef Params;

    public RequireMovingCommand(RequireMovingCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;
        
        if (target is CharacterEntity character)
        {
            if (Params.CheckVelocity == 1)
            {
                if (Params.Velocitytol == 0)
                {
                    result = !character.MovementStateContainer.Movement;
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
            result = !result;
        }

        return result;
    }
}