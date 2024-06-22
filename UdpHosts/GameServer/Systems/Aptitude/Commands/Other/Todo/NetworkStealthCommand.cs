using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class NetworkStealthCommand : ICommand
{
    private NetworkStealthCommandDef Params;

    public NetworkStealthCommand(NetworkStealthCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}