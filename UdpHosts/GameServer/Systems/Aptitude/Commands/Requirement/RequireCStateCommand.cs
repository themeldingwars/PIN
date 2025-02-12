using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using static AeroMessages.GSS.V66.Character.CharacterStateData;

namespace GameServer.Aptitude;

public class RequireCStateCommand : Command, ICommand
{
    private RequireCStateCommandDef Params;

    public RequireCStateCommand(RequireCStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

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
                result = true;
            }
            else if (Params.Incapacitated == 1 && (cstate == CharacterStatus.Incapacitated))
            {
                result = true;
            }
            else if (Params.Traumatized == 1 && (cstate == CharacterStatus.Traumatized))
            {
                result = true;
            }
            else if (Params.Ghost == 1 && (cstate == CharacterStatus.Ghost))
            {
                result = true;
            }
            else if (Params.Living == 1 && (cstate == CharacterStatus.Living))
            {
                result = true;
            }
            else if (Params.Dead == 1 && (cstate == CharacterStatus.Dead))
            {
                result = true;
            }
            else if (Params.Spawning == 1 && (cstate == CharacterStatus.Spawning))
            {
                result = true;
            }
        }
        else
        {
            Console.WriteLine($"RequireCStateCommand {Id} fails because source is not a Character. Source is {source.GetType().Name}. If this is happening, we should investigate why.");
            result = false;
        }

        return result;
    }
}