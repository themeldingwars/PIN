using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SinLinkRevealCommand : Command, ICommand
{
    private SinLinkRevealCommandDef Params;

    public SinLinkRevealCommand(SinLinkRevealCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}