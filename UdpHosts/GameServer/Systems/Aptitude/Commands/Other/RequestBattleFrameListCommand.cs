using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

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