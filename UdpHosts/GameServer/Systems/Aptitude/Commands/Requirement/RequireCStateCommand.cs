using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;
using static AeroMessages.GSS.Character.CharacterStateData;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireCStateCommand : Command, ICommand
{
    private RequireCStateCommandDef Params;

    public RequireCStateCommand(RequireCStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        // NOTE: Investigate target handling
        var source = context.Self;
        if (Params.FromInitiator == 1)
        {
            source = context.Initiator;
        }

        if (source is CharacterEntity character)
        {
            var cstate = character.CharacterState.State;

            if (Params.Respawning == 1 && (cstate == CharacterStatus.Respawning))
            {
                cmdResult = true;
            }
            else if (Params.Incapacitated == 1 && (cstate == CharacterStatus.Incapacitated))
            {
                cmdResult = true;
            }
            else if (Params.Traumatized == 1 && (cstate == CharacterStatus.Traumatized))
            {
                cmdResult = true;
            }
            else if (Params.Ghost == 1 && (cstate == CharacterStatus.Ghost))
            {
                cmdResult = true;
            }
            else if (Params.Living == 1 && (cstate == CharacterStatus.Living))
            {
                cmdResult = true;
            }
            else if (Params.Dead == 1 && (cstate == CharacterStatus.Dead))
            {
                cmdResult = true;
            }
            else if (Params.Spawning == 1 && (cstate == CharacterStatus.Spawning))
            {
                cmdResult = true;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because source is not a Character. Source is {sourceType}. If this is happening, we should investigate why.", nameof(RequireCStateCommand), Params.Id, source.GetType().Name);
            cmdResult = false;
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