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

    public override void Execute(Context context, ref CommandResult result)
    {
        // var message = new SendTipMessage() { };
        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}