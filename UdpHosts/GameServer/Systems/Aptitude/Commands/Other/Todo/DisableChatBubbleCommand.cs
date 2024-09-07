using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class DisableChatBubbleCommand : Command, ICommand
{
    private DisableChatBubbleCommandDef Params;

    public DisableChatBubbleCommand(DisableChatBubbleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}