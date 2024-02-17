using System;
using System.Linq;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AuthorizeTerminalCommand : ICommand
{
    private AuthorizeTerminalCommandDef Params;

    public AuthorizeTerminalCommand(AuthorizeTerminalCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;
        if (context.Targets.Count == 0)
        {
            Console.WriteLine($"AuthorizeTerminalCommand fails because there are no targets (There should be a target?)");
            return false;
        };

        var terminal = context.Targets.First();

        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = target as Entities.Character.CharacterEntity;
            
            if (!character.IsPlayerControlled)
            {
                Console.WriteLine($"AuthorizeTerminalCommand skips because target is not a player (should this really be happening, why did we target an NPC with this?)");
                return true;
            }

            character.SetAuthorizedTerminal(new AeroMessages.GSS.V66.Character.Controller.AuthorizedTerminalData
            {
                TerminalType = (byte)Params.TerminalType, 
                TerminalId = (byte)Params.TerminalId,
                TerminalEntityId = terminal.EntityId
            });
            
            return true;
        }

        Console.WriteLine($"AuthorizeTerminalCommand fails because self is not a character (why is it running on something other than a character?)");
        return false;
    }
}