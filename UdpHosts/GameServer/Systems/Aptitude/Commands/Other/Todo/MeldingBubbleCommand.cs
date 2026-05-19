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

    public bool Execute(Context context)
    {
        return true;
    }
}