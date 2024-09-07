using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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