using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class RequestBattleFrameListCommand : Command, ICommand
{
    private RequestBattleFrameListCommandDef Params;

    public RequestBattleFrameListCommand(RequestBattleFrameListCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            // Opens battleframe station
            character.SetAuthorizedTerminal(
                new AuthorizedTerminalData
                 {
                     TerminalId = 0, TerminalType = 8, TerminalEntityId = 0
                 });
        }

        result.SetPass();
        return;
    }
}