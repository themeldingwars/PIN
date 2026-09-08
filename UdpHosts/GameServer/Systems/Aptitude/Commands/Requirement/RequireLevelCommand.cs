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

    public bool Execute(Context context)
    {
        // See CharacterRequirement: levels belong to characters, and the character of a deployable owned
        // chain is the player that triggered it.
        var character = CharacterRequirement.Find(context, false);

        if (character == null)
        {
            // CharacterRequirement.LogNotApplicable(Logger, nameof(RequireLevelCommand), Params.Id, context);

            return true;
        }

        bool result = false;
        {
            if (Params.FrameLevel == 1)
            {
                result = character.Character_BaseController.LevelProp >= Params.Level;
            }
            else if (Params.SessionLevel == 1)
            {
                // todo
                Logger.Debug("[{Command} {CommandId}] Session level, level {Level}", nameof(RequireLevelCommand), Params.Id, Params.Level);
                result = true;
            }
        }

        return result;
    }
}