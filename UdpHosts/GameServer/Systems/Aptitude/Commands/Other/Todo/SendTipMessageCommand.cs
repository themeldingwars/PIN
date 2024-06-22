using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SendTipMessageCommand : ICommand
{
    private SendTipMessageCommandDef Params;

    public SendTipMessageCommand(SendTipMessageCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}