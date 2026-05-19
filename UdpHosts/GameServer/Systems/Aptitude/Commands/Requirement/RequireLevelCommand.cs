using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireLevelCommand : Command, ICommand
{
    private RequireLevelCommandDef Params;

    public RequireLevelCommand(RequireLevelCommandDef par)
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
            if (Params.FrameLevel == 1)
            {
                cmdResult = character.Character_BaseController.LevelProp >= Params.Level;
            }
            else if (Params.SessionLevel == 1)
            {
                // todo
                Logger.Information("[{Command} {CommandId}] Session level, level {Level}", nameof(RequireLevelCommand), Params.Id, Params.Level);
                cmdResult = true;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireLevelCommand), Params.Id);
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