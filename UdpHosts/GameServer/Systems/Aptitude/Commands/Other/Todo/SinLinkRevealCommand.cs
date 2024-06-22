using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SinLinkRevealCommand : ICommand
{
    private SinLinkRevealCommandDef Params;

    public SinLinkRevealCommand(SinLinkRevealCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}