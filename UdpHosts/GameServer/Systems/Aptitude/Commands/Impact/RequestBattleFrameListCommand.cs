using AeroMessages.GSS.V66.Character.Controller;
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

    public bool Execute(Context context)
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

        return true;
    }
}