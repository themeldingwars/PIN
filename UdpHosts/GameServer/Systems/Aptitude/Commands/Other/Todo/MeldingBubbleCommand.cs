using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class MeldingBubbleCommand : Command, ICommand
{
    private MeldingBubbleCommandDef Params;

    public MeldingBubbleCommand(MeldingBubbleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}