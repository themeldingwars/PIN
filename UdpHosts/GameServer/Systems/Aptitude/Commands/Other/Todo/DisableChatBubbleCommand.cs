using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class DisableChatBubbleCommand : Command, ICommand
{
    private DisableChatBubbleCommandDef Params;

    public DisableChatBubbleCommand(DisableChatBubbleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}