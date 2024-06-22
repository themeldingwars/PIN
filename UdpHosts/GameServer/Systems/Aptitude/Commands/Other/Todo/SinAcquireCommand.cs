using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SinAcquireCommand : ICommand
{
    private SinAcquireCommandDef Params;

    public SinAcquireCommand(SinAcquireCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}