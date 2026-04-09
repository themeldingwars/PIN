using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class AuthorizeTerminalCommand : Command, ICommand
{
    private AuthorizeTerminalCommandDef Params;

    public AuthorizeTerminalCommand(AuthorizeTerminalCommandDef par)
: base(par)
    {
        Params = par;
    }

    // self is terminal, target is interacting player
    public bool Execute(Context context)
    {
        var terminal = context.Self;
        if (context.Targets.Count == 0)
        {
            Logger.Warning("{Command} {CommandId} fails because there are no targets (There should be a target?)", nameof(AuthorizeTerminalCommand), Params.Id);
            return false;
        }

        var target = context.Targets.Peek();

        if (target is CharacterEntity character)
        {
            if (!character.IsPlayerControlled)
            {
                Logger.Information("{Command} {CommandId} skips because target is not a player (should this really be happening, why did we target an NPC with this?)", nameof(AuthorizeTerminalCommand), Params.Id);
                return true;
            }

            Logger.Information("{Command} {CommandId} Authorized terminal {TerminalType}, {terminal}", nameof(AuthorizeTerminalCommand), Params.Id, Params.TerminalType, terminal);

            character.SetAuthorizedTerminal(new AuthorizedTerminalData
            {
                TerminalType = (byte)Params.TerminalType,
                TerminalId = (byte)Params.TerminalId,
                TerminalEntityId = terminal.AeroEntityId.Backing
            });
            
            return true;
        }

        Logger.Warning("{Command} {CommandId} fails because target is not a character (why is it running on something other than a character?)", nameof(AuthorizeTerminalCommand), Params.Id);
        return false;
    }
}