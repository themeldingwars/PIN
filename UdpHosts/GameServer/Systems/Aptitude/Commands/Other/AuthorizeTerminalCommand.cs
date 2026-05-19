using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class AuthorizeTerminalCommand : Command, ICommand
{
    private AuthorizeTerminalCommandDef Params;

    public AuthorizeTerminalCommand(AuthorizeTerminalCommandDef par)
: base(par)
    {
        Params = par;
    }

    // self is terminal, target is interacting player
    public override void Execute(Context context, ref CommandResult result)
    {
        var terminal = context.Self;
        if (context.Targets.Count == 0)
        {
            Logger.Warning("{Command} {CommandId} fails because there are no targets (There should be a target?)", nameof(AuthorizeTerminalCommand), Params.Id);
            result.SetFail();
            return;
        }

        var target = context.Targets.Peek();

        if (target is CharacterEntity character)
        {
            if (!character.IsPlayerControlled)
            {
                Logger.Information("{Command} {CommandId} skips because target is not a player (should this really be happening, why did we target an NPC with this?)", nameof(AuthorizeTerminalCommand), Params.Id);
                result.SetPass();
                return;
            }

            Logger.Information("{Command} {CommandId} Authorized terminal {TerminalType}, {terminal}", nameof(AuthorizeTerminalCommand), Params.Id, Params.TerminalType, terminal);

            character.SetAuthorizedTerminal(new AuthorizedTerminalData
            {
                TerminalType = (byte)Params.TerminalType,
                TerminalId = (byte)Params.TerminalId,
                TerminalEntityId = terminal.AeroEntityId.Backing
            });

            result.SetPass();
            return;
        }

        Logger.Warning("{Command} {CommandId} fails because target is not a character (why is it running on something other than a character?)", nameof(AuthorizeTerminalCommand), Params.Id);
        result.SetFail();
        return;
    }

    public override void Reset(Context context)
    {
    }
}