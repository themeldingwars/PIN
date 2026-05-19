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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}