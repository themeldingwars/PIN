using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class DisableChatBubbleCommand : ICommand
{
    private DisableChatBubbleCommandDef Params;

    public DisableChatBubbleCommand(DisableChatBubbleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}