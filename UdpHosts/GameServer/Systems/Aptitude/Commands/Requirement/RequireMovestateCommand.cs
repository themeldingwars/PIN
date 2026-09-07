using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireMovestateCommand : Command, ICommand
{
    private RequireMovestateCommandDef Params;

    public RequireMovestateCommand(RequireMovestateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // The movement state lives on the character, and for an ability owned by a deployable (a glider pad
        // keeping its launch effect alive) the character is the player who triggered it, not Self.
        var character = CharacterRequirement.Find(context, false);

        if (character == null)
        {
            // No character in this activation: the requirement cannot be answered, so it cannot be violated.
            // Returning early (instead of falling through and negating the empty result) keeps a deployable
            // owned effect from being removed and re-applied every tick.
            CharacterRequirement.LogNotApplicable(Logger, nameof(RequireMovestateCommand), Params.Id, context);

            return true;
        }

        bool result = false;
        {
            var movestate = character.MovementStateContainer.Movestate;

            if (Params.Standing == 1 && (movestate == Movestate.Standing))
            {
                result = true;
            }
            else if (Params.Running == 1 && (movestate == Movestate.Running))
            {
                result = true;
            }
            else if (Params.Falling == 1 && (movestate == Movestate.Falling))
            {
                result = true;
            }
            else if (Params.Sliding == 1 && (movestate == Movestate.Sliding))
            {
                result = true;
            }
            else if (Params.Walking == 1 && (movestate == Movestate.Walking))
            {
                result = true;
            }
            else if (Params.Jetpack == 1 && (movestate == Movestate.Jetpack))
            {
                result = true;
            }
            else if (Params.Gliding == 1 && (movestate == Movestate.Glider))
            {
                result = true;
            }
            else if (Params.Thruster == 1 && (movestate == Movestate.GliderThrusters))
            {
                result = true;
            }
            else if (Params.Stall == 1 && (movestate == Movestate.GliderStalling))
            {
                result = true;
            }
            else if (Params.KnockdownOnground == 1 && (movestate == Movestate.Knockdown))
            {
                result = true;
            }
            else if (Params.KnockdownFalling == 1 && (movestate == Movestate.KnockdownFalling))
            {
                result = true;
            }
            else if (Params.JetpackSprint == 1 && (movestate == Movestate.JetpackSprint))
            {
                result = true;
            }
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}