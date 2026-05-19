using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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