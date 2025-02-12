using AeroMessages.GSS.V66.Generic;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SendTipMessageCommand : Command, ICommand
{
    private SendTipMessageCommandDef Params;

    public SendTipMessageCommand(SendTipMessageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // var message = new SendTipMessage() { };
        return true;
    }
}