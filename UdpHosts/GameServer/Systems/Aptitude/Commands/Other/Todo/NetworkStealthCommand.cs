using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class NetworkStealthCommand : Command, ICommand
{
    private NetworkStealthCommandDef Params;

    public NetworkStealthCommand(NetworkStealthCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}